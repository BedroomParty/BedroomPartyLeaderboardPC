using IPA.Utilities.Async;
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
using Zenject;

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
            Plugin.Log.Info("COLLECTING PLAYER INFO");
            TaskCompletionSource<(string, string)> taskCompletionSource = new TaskCompletionSource<(string, string)>();
            if (File.Exists(Constants.STEAM_API_PATH))
            {
                Plugin.Log.Info("STEAM USER");
                (string steamID, string steamName) = OculusSkillIssue();
                taskCompletionSource.SetResult((steamID, steamName));
            }
            else
            {
                Plugin.Log.Info("OCULUS USER");

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

            const string url = "http://168.138.9.99:5000/api/login/";
            _leaderboardView.userIDHere.text = id;
            var idBytes = Encoding.UTF8.GetBytes(id);
            var authKey = Convert.ToBase64String(idBytes);


            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", authKey);
                    httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    string requestBody = $"{{\"ID\": \"{id}\"}}";


                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");


                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    bool isAuthed = response.StatusCode == HttpStatusCode.OK;
                    await Task.Delay(2000);
                    callback((isAuthed, username));
                    await Task.Delay(3000);
                    _panelView.prompt_loader.SetActive(false);
                    _panelView.promptText.gameObject.SetActive(false);
                }
                catch (HttpRequestException)
                {
                    _panelView.prompt_loader.SetActive(false);
                    _panelView.promptText.text = "<color=red>Error Authenticating</color>";
                    callback((false, username));
                }
            }
        }

        public void GetAuthStatus(Action<(bool, string)> callback)
        {
            Task.Run(() => GetAuth(callback));
        }
    }
}
