using BeatSaberMarkupLanguage;
using BedroomPartyLeaderboard.UI.Leaderboard;
using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static BedroomPartyLeaderboard.Utils.UIUtils;

namespace BedroomPartyLeaderboard.Utils
{
    internal class PlayerUtils
    {
        [Inject] PanelView _panelView;
        [Inject] LeaderboardView _leaderboardView;
        [Inject] UIUtils _uiUtils;

        protected private bool _isAuthed = false;
        public PlayerInfo localPlayerInfo;

        public bool isAuthed
        {
            get { return _isAuthed; }
        }

        public bool currentlyAuthing;

        private async Task<PlayerInfo> GetSteamInfo()
        {
            await WaitUntil(() => SteamManager.Initialized);

            string authToken = (await new SteamPlatformUserModel().GetUserAuthToken()).token;

            PlayerInfo steamInfo = await Task.Run(() =>
            {
                Steamworks.CSteamID steamID = Steamworks.SteamUser.GetSteamID();
                string playerId = steamID.m_SteamID.ToString();
                string playerName = Steamworks.SteamFriends.GetPersonaName();
                return new PlayerInfo(playerName, playerId, Constants.Base64Encode(authToken));
            });
            return steamInfo;
        }

        public Task<PlayerInfo> GetPlayerInfo()
        {
            TaskCompletionSource<PlayerInfo> taskCompletionSource = new TaskCompletionSource<PlayerInfo>();
            string playerId = "";
            string playerName = "";
            string authKey = "";

            if (File.Exists(Constants.STEAM_API_PATH))
            {
                taskCompletionSource.SetResult(Task.Run(() => GetSteamInfo()).Result);
            }
            else
            {
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user =>
                {
                    Oculus.Platform.Users.GetUserProof().OnComplete(userProofMessage =>
                    {
                        if (!userProofMessage.IsError)
                        {
                            Oculus.Platform.Users.GetAccessToken().OnComplete(authTokenMessage =>
                            {
                                if (!authTokenMessage.IsError)
                                {
                                    playerId = user.Data.ID.ToString();
                                    playerName = user.Data.OculusID;
                                    authKey = userProofMessage.Data.Value + "," + authTokenMessage.Data;
                                    taskCompletionSource.SetResult(new PlayerInfo(playerId, playerName, Constants.Base64Encode(authKey)));
                                }
                                else
                                {
                                    taskCompletionSource.SetException(new Exception("Failed to get access token."));
                                }
                            });
                        }
                        else
                        {
                            taskCompletionSource.SetException(new Exception("Failed to get user proof."));
                        }
                    });
                });
            }
            return taskCompletionSource.Task;
        }



        private string GetLoginString(string userID)
        {
            JObject user = new JObject
            {
                { "userID", userID }
            };

            return user.ToString();
        }


        private async Task GetAuth(Action<bool> callback)
        {
            PlayerInfo _localPlayerInfo = Task.Run(() => GetPlayerInfo()).Result;
            localPlayerInfo = _localPlayerInfo;
            _panelView.playerUsername.text = localPlayerInfo.username;
            _isAuthed = true;
            callback(true);
            _uiUtils.GetCoolMaterialAndApply();
            return;

            using (var httpClient = Plugin.httpClient)
            {
                int x = 0;
                while (x < 3)
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", localPlayerInfo.authKey);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        string requestBody = GetLoginString(_localPlayerInfo.userID);
                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.AUTH_END_POINT, content).ConfigureAwait(false);
                        _isAuthed = response.StatusCode == HttpStatusCode.OK;

                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        JObject jsonResponse = JObject.Parse(responseContent);

                        if (_isAuthed)
                        {
                            _panelView.prompt_loader.SetActive(false);
                            _panelView.promptText.gameObject.SetActive(false);
                            callback(true);
                            return;
                        }
                        _panelView.promptText.text = $"<color=red>Error Authenticating... attempt {x + 1} of 3</color>";
                        await Task.Delay(500);
                        x++;
                    }
                    catch (HttpRequestException)
                    {
                        _panelView.promptText.text = $"<color=red>Error Authenticating... attempt {x + 1} of 3</color>";
                        x++;
                        await Task.Delay(5000);
                    }
                    x++;
                }
                if (x < 2)
                {
                    callback(false);
                }
            }
        }


        public async void GetAuthStatus(Action<bool> callback)
        {
            await Task.Run(() => GetAuth(callback));
        }


        public void LoginUser()
        {
            Task.Run(() => GetAuthStatus(result =>
            {
                if (_isAuthed)
                {
                    _panelView.prompt_loader.SetActive(false);
                    _panelView.promptText.text = $"<color=green>Successfully signed in!</color>";
                    if (_leaderboardView.currentDifficultyBeatmap != null)
                    {
                        _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                        _leaderboardView.UpdatePageButtons();
                    }
                    _panelView.playerAvatar.SetImage("https://cdn.assets.beatleader.xyz/76561199077754911R34.png");
                    _panelView.playerAvatarLoading.gameObject.SetActive(false);

                    if (Constants.isStaff(localPlayerInfo.userID))
                    {
                        RainbowAnimation rainbowAnimation = _panelView.playerUsername.gameObject.AddComponent<RainbowAnimation>();
                        rainbowAnimation.speed = 0.35f;
                    }
                    else
                    {
                        RainbowAnimation rainbowAnimation = _panelView.playerUsername.gameObject.GetComponent<RainbowAnimation>();
                        if (rainbowAnimation != null)
                        {
                            UnityEngine.Object.Destroy(rainbowAnimation);
                        }
                        _panelView.playerUsername.color = Color.white;
                    }
                }
                else
                {
                    _panelView.promptText.text = "<color=red>Error Authenticating</color>";
                    _panelView.prompt_loader.SetActive(false);
                    Plugin.Log.Error("Not authenticated!");
                }
            }));
        }

        public struct PlayerInfo
        {
            public string username;
            public string userID;
            public readonly string authKey;

            public PlayerInfo(string username, string userID, string authKey)
            {
                this.authKey = authKey;
                this.username = username;
                this.userID = userID;
            }
        }


        // from scoresaber yoink teehee
        internal static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }
    }
}
