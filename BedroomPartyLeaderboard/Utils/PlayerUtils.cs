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

        private async Task<PlayerInfo> GetSteamInfoAsync()
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

        public Task<PlayerInfo> GetPlayerInfoAsync()
        {
            TaskCompletionSource<PlayerInfo> taskCompletionSource = new();
            string playerId = "";
            string playerName = "";
            string authKey = "";

            if (File.Exists(Constants.STEAM_API_PATH))
            {
                PlayerInfo silly = Task.Run(() => GetSteamInfoAsync()).Result;
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
            if (NullCheckFilePath(Constants.API_KEY_PATH))
            {
                string[] sillyvar = Constants.Base64Decode(File.ReadAllText(Constants.API_KEY_PATH)).Split(',');
                if (sillyvar[1] != playerId)
                {
                    // the player id in the file doesn't match the current player id, so we can't auth
                    taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, ""));
                    return taskCompletionSource.Task;
                }
                authKey = sillyvar[0];
            }
            else
            {
                // no file exists, so we can't auth
                taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, ""));
                return taskCompletionSource.Task;
            }

            // if we get here, we have a valid auth key
            taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, authKey, ""));
            return taskCompletionSource.Task;
        }


        private bool NullCheckFilePath(string path)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                return false;
            }
            return true;
        }

        private string GetLoginString(string userID)
        {
            JObject user = new()
            {
                { "id", userID },
            };

            return user.ToString();
        }

        private async Task GetAuthAsync()
        {
            PlayerInfo _localPlayerInfo = await GetPlayerInfoAsync();
            localPlayerInfo = _localPlayerInfo;
            _panelView.playerUsername.text = localPlayerInfo.username;

            if(localPlayerInfo.authKey == null)
            {
                _isAuthed = false;
                return;
            }
            using HttpClient httpClient = Plugin.httpClient;
            int x = 0;
            while (x < 3)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", localPlayerInfo.authKey);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    string requestBody = GetLoginString(_localPlayerInfo.userID);
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
                        throw new Exception("Error Authenticating, RESTART GAME.");
                    }

                    if (_isAuthed)
                    {
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.gameObject.SetActive(false);
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
                throw new Exception("Error Authenticating.");
            }
        }

        public async Task GetAuthStatusAsync()
        {
            _leaderboardView.SetErrorState(false, "");
            currentlyAuthing = true;
            try
            {
                await GetAuthAsync();
                currentlyAuthing = false;
                if (_leaderboardView.currentDifficultyBeatmap != null)
                {
                    _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                    _leaderboardView.UpdatePageButtons();
                }
                _panelView.prompt_loader.SetActive(false);
                _panelView.promptText.text = $"<color=green>Successfully signed in!</color>";
                _panelView.playerAvatar.SetImage($"https://api.thebedroom.party/user/{localPlayerInfo.userID}/avatar");
                _panelView.playerAvatarLoading.gameObject.SetActive(false);
                _panelView.promptText.gameObject.SetActive(false);
                _panelView.prompt_loader.SetActive(false);
                await Task.Delay(5000);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
                currentlyAuthing = false;
                _leaderboardView.SetErrorState(true, "Failed to Auth");
            }
        }

        public async Task LoginUserAsync()
        {
            try
            {
                await GetAuthStatusAsync();
                if (_isAuthed)
                {


                    if (await Task.Run(() => Constants.isStaff(localPlayerInfo.userID)))
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
                    _leaderboardView.SetErrorState(true, "Failed to Auth");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("LoginUserAsync failed: " + ex.Message);
                _leaderboardView.SetErrorState(true, "Failed to Auth");
            }
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

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }
    }
}
