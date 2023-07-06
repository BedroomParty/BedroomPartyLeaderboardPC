using IPA.Utilities.Async;
using ModestTree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QSLeaderboard.UI.Leaderboard;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static QSLeaderboard.Utils.UIUtils;

namespace QSLeaderboard.Utils
{

    internal class PlayerUtils
    {
        [Inject] PanelView _panelView;
        [Inject] LeaderboardView _leaderboardView;
        [Inject] UIUtils _uiUtils;
        public (string, string) OculusSkillIssue()
        {
            var steamID = "0";
            var steamName = "loser";
            steamID = Steamworks.SteamUser.GetSteamID().ToString();
            steamName = Steamworks.SteamFriends.GetPersonaName();
            Plugin.platformID = steamID;
            return (steamID, steamName);
        }

        public Task<(string, string)> GetPlayerInfo()
        {
            TaskCompletionSource<(string, string)> taskCompletionSource = new TaskCompletionSource<(string, string)>();
            if (File.Exists(Constants.STEAM_API_PATH))
            {
                (string steamID, string steamName) = OculusSkillIssue();
                taskCompletionSource.SetResult((steamID, steamName));
            }
            else
            {
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user =>
                {
                    Plugin.platformID = user.Data.ID.ToString();
                    taskCompletionSource.SetResult((user.Data.ID.ToString(), user.Data.OculusID));
                });
            }

            return taskCompletionSource.Task;
        }

        // CODE


        private async Task GetAuthCode(int code, Action<bool> callback)
        {
            _panelView.prompt_loader.SetActive(true);
            _panelView.promptText.gameObject.SetActive(true);
            _panelView.promptText.text = "Creating User...";
            (string id, string username) = await GetPlayerInfo();


            using (var httpClient = new HttpClient())
            {
                int x = 0;
                while (x < 2)
                {
                    try
                    {
                        await Task.Delay(1);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        string requestBody = getLoginStringCode(id, code);

                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.USER_URL + "/link", content);
                        bool isAuthed = response.StatusCode == HttpStatusCode.OK;
                        if (isAuthed)
                        {
                            // Parse the response and extract the API key
                            string responseContent = await response.Content.ReadAsStringAsync();
                            JObject jsonResponse = JObject.Parse(responseContent);
                            string apiKey;
                            if (jsonResponse.TryGetValue("key", out JToken apiKeyToken))
                            {
                                apiKey = apiKeyToken.Value<string>();
                                Plugin.apiKey = apiKey;
                                if (!string.IsNullOrEmpty(apiKey))
                                {
                                    if (!Directory.Exists(Constants.BALL_PATH))
                                    {
                                        Directory.CreateDirectory(Constants.BALL_PATH);
                                    }
                                    if (!File.Exists(Constants.BALL_PATH + "apiKey.txt"))
                                    {
                                        using (File.Create(Constants.BALL_PATH + "apiKey.txt")) { }
                                    }
                                    string apiKeyFilePath = Constants.BALL_PATH + "apiKey.txt";

                                    using (StreamWriter sw = new(apiKeyFilePath))
                                    {
                                        if (!apiKey.IsEmpty())
                                        {
                                            await sw.WriteAsync(apiKey);
                                        }
                                        else
                                        {
                                            Plugin.Log.Error("Failed to parse API key from the response.");
                                        }
                                    }
                                }
                                else
                                {
                                    Plugin.Log.Error("Failed to parse API key from the response.");
                                }
                            }
                            else
                            {
                                Plugin.Log.Error("API key not found in the response.");

                            }
                            callback(isAuthed);
                            break;
                        }
                        await Task.Delay(2000);
                        _panelView.promptText.text = $"<color=red>Error Creating User... attempt {x + 1} of 3</color>";
                        x++;
                    }
                    catch (HttpRequestException ex)
                    {
                        _panelView.promptText.text = $"<color=red>Error Creating User... attempt {x + 1} of 3</color>";
                        Plugin.Log.Error($"HttpRequestException: {ex.Message}");
                        x++;
                        await Task.Delay(5000);
                    }
                    catch (JsonException ex)
                    {
                        _panelView.promptText.text = $"<color=red>Error Creating User... attempt {x + 1} of 3</color>";
                        Plugin.Log.Error($"JsonException: {ex.Message}");
                        x++;
                        await Task.Delay(5000);
                    }
                }
                if (x == 2)
                {
                    callback(false);
                }
            }
        }


        private string getLoginStringCode(string id, int code)
        {
            var Data = new JObject
            {
                { "id", id },
                { "code", code.ToString()}
            };

            return Data.ToString();
        }

        private string getLoginStringKey(string id)
        {
            var Data = new JObject
            {
                { "id", id },
            };

            return Data.ToString();
        }

        public void GetAuthStatusCode(int code, Action<bool> callback)
        {
            Task.Run(() => GetAuthCode(code, callback));
        }



        // KEY


        private async Task GetAuthKey(string apiKey, Action<(bool, string)> callback)
        {
            _panelView.prompt_loader.SetActive(true);
            _panelView.promptText.gameObject.SetActive(true);
            _panelView.promptText.text = "Authenticating...";

            (string id, string username) = await GetPlayerInfo();
            _panelView.playerUsername.text = username;
            string discordID;
            string usernameTemp = "Error";

            using (var httpClient = new HttpClient())
            {
                int x = 0;
                while (x < 3)
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiKey);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        string requestBody = getLoginStringKey(id);
                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.AUTH_END_POINT, content).ConfigureAwait(false);
                        bool isAuthed = response.StatusCode == HttpStatusCode.OK;

                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        JObject jsonResponse = JObject.Parse(responseContent);

                        if (isAuthed)
                        {
                            Plugin.Authed = true;
                            _panelView.prompt_loader.SetActive(false);
                            _panelView.promptText.gameObject.SetActive(false);

                            if (jsonResponse.TryGetValue("ID", out JToken discordIDToken))
                            {
                                discordID = discordIDToken.Value<string>();
                                Plugin.discordID = discordID;
                            }
                            else
                            {
                                Plugin.Log.Error("Discord ID key not found in the response.");
                            }

                            if (jsonResponse.TryGetValue("Username", out JToken usernameToken))
                            {
                                usernameTemp = usernameToken.Value<string>();
                                Plugin.userName = usernameTemp;
                                _panelView.playerUsername.text = usernameTemp;
                            }
                            else
                            {
                                Plugin.Log.Error("Username key not found in the response.");
                            }

                            callback((true, usernameTemp));
                            return;
                        }
                        _panelView.promptText.text = $"<color=red>Error Authenticating... attempt {x + 1} of 3</color>";
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
                if (x == 3)
                {
                    callback((false, username));
                }
            }
        }


        public void GetAuthStatusKey(string key, Action<(bool, string)> callback)
        {
            Task.Run(() => GetAuthKey(key, callback));
        }




        public void LoginCode(int code)
        {
            GetAuthStatusCode(code, result =>
            {
                if (result)
                {
                    _leaderboardView.loginButton.gameObject.SetActive(false);
                    _leaderboardView.loginKeyboard.gameObject.SetActive(false);
                    LoginKey(Plugin.apiKey);
                }
                else
                {
                    _panelView.promptText.text = "<color=red>Error Creating User</color>";
                    _panelView.prompt_loader.SetActive(false);
                    Plugin.Log.Error("Not created user!");
                }
            });
        }

        public void LoginKey(string apiKey)
        {
            GetAuthStatusKey(apiKey, result =>
            {
                bool isAuthenticated = result.Item1;
                string username = result.Item2;

                if (isAuthenticated)
                {
                    Plugin.Authed = true;
                    Plugin.userName = username;
                    _panelView.prompt_loader.SetActive(false);
                    _panelView.promptText.text = $"<color=green>Successfully signed {username} in!</color>";
                    if (_leaderboardView.currentDifficultyBeatmap != null)
                    {
                        _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                        _leaderboardView.UpdatePageButtons();
                    }
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetProfilePic(_panelView.playerAvatar, Constants.profilePictureLink(Plugin.discordID)));

                    if (Constants.isStaff(Plugin.discordID))
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
                    Plugin.Log.Error("Not authenticated! Username: " + username);
                }
            });
        }


    }
}
