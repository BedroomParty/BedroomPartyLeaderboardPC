using BedroomPartyLeaderboard.UI.Leaderboard;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class RequestUtils
    {
        [Inject] private readonly LeaderboardData _leaderboardData;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly AuthenticationManager _authenticationManager;

        public async Task GetLeaderboardData((string, int, string) balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int)> callback)
        {
            using HttpClient httpClient = new();
            try
            {
                string requestString = getLBDownloadJSON(balls, page, _leaderboardView.sortMethod);

                HttpResponseMessage response = await httpClient.GetAsync(requestString);

                int scorecount = 0;
                int totalPages = 0;
                List<LeaderboardData.LeaderboardEntry> data = new();

                if (!response.IsSuccessStatusCode)
                {
                    callback((false, data, 0));
                    return;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                LeaderboardData.BPLeaderboard leaderboardData = JsonConvert.DeserializeObject<LeaderboardData.BPLeaderboard>(jsonResponse);
                scorecount = leaderboardData.scoreCount;
                totalPages = Mathf.CeilToInt((float)scorecount / 10);
                data = leaderboardData.scores;

                callback((true, data, totalPages));
                return;
            }
            catch (HttpRequestException e)
            {
                Plugin.Log.Error("EXCEPTION: " + e.ToString());
                callback((false, null, 0));
            }
        }

        private string getLBDownloadJSON((string, int, string) balls, int page, string sort)
        {
            string Data = $"{Constants.LEADERBOARD_DOWNLOAD_END_POINT(balls.Item1)}?char={balls.Item3}&diff={balls.Item2}&sort={sort}&limit=10&page={page}&id={_authenticationManager._localPlayerInfo.userID}";
            return Data;
        }

        public void GetBeatMapData((string, int, string) balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int)> callback)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => GetLeaderboardData(balls, page, callback));
        }

        public void SetBeatMapData(string uploadJson, Action<bool> callback)
        {
            _leaderboardView.hasClickedOffResultsScreen = false;
            UnityMainThreadTaskScheduler.Factory.StartNew(() => UploadLeaderboardData(uploadJson, callback));
        }


        internal bool isUploading = false;
        private async Task UploadLeaderboardData(string balls, Action<bool> callback)
        {

            if (DateTime.Now.Millisecond > _authenticationManager._localPlayerInfo.sessionExpiry) return;
            _leaderboardView.hasClickedOffResultsScreen = false;
            using HttpClient httpClient = new();
            int x = 0;
            isUploading = true;
            while (x < 3)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _authenticationManager._localPlayerInfo.tempKey);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    HttpContent content = new StringContent(balls, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_UPLOAD_END_POINT(balls), content);

                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        callback(response.IsSuccessStatusCode);
                        isUploading = false;
                        break;
                    }

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        callback(response.IsSuccessStatusCode);
                        isUploading = false;
                        break;
                    }
                }
                catch (HttpRequestException e)
                {
                    Plugin.Log.Error("EXCEPTION: " + e.ToString());
                    callback(false);
                }
                x++;
            }
            if (x == 2)
            {
                callback(false);
                await Task.Delay(3000);
            }
        }


    }
}
