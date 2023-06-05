using IPA.Utilities.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QSLeaderboard.Utils
{
    internal class PlayerUtils
    {
        public (string, string) OculusSkillIssue()
        {
            var steamID = "0";
            var steamName = "loser";
            //steamID = Steamworks.SteamUser.GetSteamID().ToString();
            //steamName = Steamworks.SteamFriends.GetPersonaName();
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
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user => taskCompletionSource.SetResult((user.Data.ID.ToString(), user.Data.OculusID)));
            }
            return taskCompletionSource.Task;
        }

        private async Task GetAuth(Action<(bool, string)> callback)
        {
            (string id, string username) = await GetPlayerInfo();
            using (var httpClient = new HttpClient())
            {
                var url = $"http://168.138.9.99:5000/api/login?id={id}";
                try
                {
                    var response = await httpClient.GetAsync(url);
                    callback((response.IsSuccessStatusCode, username));
                }
                catch (HttpRequestException)
                {
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
