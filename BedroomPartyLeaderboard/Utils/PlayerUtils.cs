using BeatSaberMarkupLanguage;
using BedroomPartyLeaderboard.UI.Leaderboard;
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
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly UIUtils _uiUtils;

        private protected bool _isAuthed = false;
        public PlayerInfo localPlayerInfo;

        public bool IsAuthed => _isAuthed;

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
                return new PlayerInfo(playerName, playerId, authToken, "");
            });
            return steamInfo;
        }

        public Task<PlayerInfo> GetPlayerInfo()
        {
            TaskCompletionSource<PlayerInfo> taskCompletionSource = new();
            string playerId = "";
            string playerName = "";
            string authKey = "";

            if (File.Exists(Constants.STEAM_API_PATH))
            {
                PlayerInfo silly = Task.Run(() => GetSteamInfo()).Result;
                playerId = silly.userID;
                playerName = silly.username;
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
            authKey = File.ReadAllText(Constants.API_KEY_PATH);
            taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, authKey, ""));
            return taskCompletionSource.Task;
        }

        private string GetLoginString(string userID, string apiKey)
        {
            JObject user = new()
            {
                { "id", long.Parse(userID) },
            };

            return user.ToString();
        }

        private async Task GetAuth(Action<bool> callback)
        {
            PlayerInfo _localPlayerInfo = await Task.Run(() => GetPlayerInfo());
            localPlayerInfo = _localPlayerInfo;
            _panelView.playerUsername.text = localPlayerInfo.username;
            _uiUtils.GetCoolMaterialAndApply();

            using HttpClient httpClient = new();
            int x = 0;
            while (x < 3)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", localPlayerInfo.authKey);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    string requestBody = GetLoginString(_localPlayerInfo.userID, _localPlayerInfo.authKey);
                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(Constants.AUTH_END_POINT, content);
                    _isAuthed = response.StatusCode == HttpStatusCode.OK;

                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(responseContent);

                    if (jsonResponse.TryGetValue("sessionKey", out JToken silly))
                    {
                        localPlayerInfo.tempKey = silly.Value<string>();
                    }
                    else
                    {
                        callback(false);
                        _panelView.promptText.text = $"<color=red>Error Authenticating, RESTART GAME.</color>";
                        return;
                    }

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


        public async void GetAuthStatus(Action<bool> callback)
        {
            currentlyAuthing = true;
            try
            {
                await Task.Run(() => GetAuth(callback));
                currentlyAuthing = false;
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
                currentlyAuthing = false;
                _leaderboardView.SetErrorState(true, "Failed to Auth\nSpeecil is silly :3");
            }
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
                    _panelView.playerAvatar.SetImage($"https://api.thebedroom.party/user/{localPlayerInfo.userID}/avatar");
                    _panelView.playerAvatarLoading.gameObject.SetActive(false);

                    if (Task.Run(() => Constants.isStaff(localPlayerInfo.userID)).Result)
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
                    Task.Delay(2000);
                    _panelView.promptText.gameObject.SetActive(false);
                    _panelView.prompt_loader.SetActive(false);
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
            internal string tempKey;

            public PlayerInfo(string username, string userID, string authKey, string tempKey)
            {
                this.authKey = authKey;
                this.username = username;
                this.userID = userID;
                this.tempKey = tempKey;
            }
        }


        // from scoresaber yoink teehee
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            Task waitTask = Task.Run(async () =>
            {
                while (!condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }
    }
}
