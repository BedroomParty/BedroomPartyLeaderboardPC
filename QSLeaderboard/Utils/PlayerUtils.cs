using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using QSLeaderboard.UI.Leaderboard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using Zenject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QSLeaderboard.Utils
{

    internal class PlayerUtils
    {
        [Inject] PanelView _panelView;
        [Inject] LeaderboardView _leaderboardView;
        public (string, string) OculusSkillIssue()
        {
            var steamID = "0";
            var steamName = "loser";
            steamID = Steamworks.SteamUser.GetSteamID().ToString();
            steamName = Steamworks.SteamFriends.GetPersonaName();
            Plugin.userID = steamID;
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
                    Plugin.userID = user.Data.ID.ToString();
                    taskCompletionSource.SetResult((user.Data.ID.ToString(), user.Data.OculusID));
                });
            }
        
            return taskCompletionSource.Task;
        }


        private async Task GetAuth(Action<(bool, string)> callback)
        {
            _panelView.prompt_loader.SetActive(true);
            _panelView.promptText.gameObject.SetActive(true);
            _panelView.promptText.text = "Authenticating...";
            (string id, string username) = await GetPlayerInfo();
            _panelView.playerUsername.text = username;

            _leaderboardView.userIDHere.text = id;
            var idBytes = Encoding.UTF8.GetBytes(id);
            var authKey = Convert.ToBase64String(idBytes);


            using (var httpClient = new HttpClient())
            {
                int x = 0;
                while(x < 2)
                {
                    try
                    {
                        Plugin.Log.Notice($"LOGIN ATTEMPT {x + 1}");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authKey);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        string requestBody = getLoginString(id);


                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.AUTH_END_POINT, content);
                        bool isAuthed = response.StatusCode == HttpStatusCode.OK;
                        if(isAuthed)
                        {
                            await Task.Delay(2000);
                            callback((isAuthed, username));
                            await Task.Delay(3000);
                            _panelView.prompt_loader.SetActive(false);
                            _panelView.promptText.gameObject.SetActive(false);
                            break;
                        }
                        await Task.Delay(2000);
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
                if (x == 2)
                {
                    callback((false, username));
                }
            }
        }

        private string getLoginString(string id)
        {
            var Data = new JObject
            {
                { "ID", id }
            };

            return Data.ToString();
            return string.Empty;
        }

        public void GetAuthStatus(Action<(bool, string)> callback)
        {
            Task.Run(() => GetAuth(callback));
        }
    }
}
