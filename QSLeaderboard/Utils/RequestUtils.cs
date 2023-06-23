using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using QSLeaderboard.UI.Leaderboard;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace QSLeaderboard.Utils
{
    internal class RequestUtils
    {
        [Inject] private LeaderboardData _leaderboardData;
        [Inject] private LeaderboardView _leaderboardView;
        [Inject] private PanelView _panelView;
        private async Task GetLeaderboardData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int, int, float)> callback)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    string requestBody = getLBDownloadJSON(balls, page, 10);


                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");


                    HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_DOWNLOAD_END_POINT, content);

                    if (response.IsSuccessStatusCode)
                    {
                        //var jsonResponse = await response.Content.ReadAsStringAsync();
                        //var leaderboardData = _leaderboardData.LoadBeatMapInfo(jsonResponse);

                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Plugin.Log.Info(jsonResponse.ToString());
                        JObject jsonObject = JObject.Parse(jsonResponse);
                        Plugin.Log.Info("JSON OBJECT PARSE");
                        int rank = jsonObject["GlobalRank"].Value<int>();
                        Plugin.Log.Info("GlobalRank PARSE");
                        int? scorecount = jsonObject["ScoreCount"]?.Value<int>();
                        Plugin.Log.Info("ScoreCount PARSE");
                        float stars = jsonObject["Stars"].Value<float>();
                        Plugin.Log.Info("Stars PARSE");
                        var totalPages = Mathf.CeilToInt((float)(scorecount ?? 0) / 10);
                        Plugin.Log.Info("totalPages calc");


                        Plugin.Log.Info($"Rank: {rank}");
                        Plugin.Log.Info($"stars: {stars}");
                        Plugin.Log.Info($"totalPages: {totalPages}");

                        JArray scoresArray = jsonObject["Scores"].Value<JArray>();
                        Plugin.Log.Info(scoresArray[0].ToString());
                        Plugin.Log.Info(scoresArray[0]["PP"].Value<float>().ToString());
                        var leaderboardData = _leaderboardData.LoadBeatMapInfo(scoresArray);
                        callback((response.IsSuccessStatusCode, leaderboardData, rank, totalPages, stars));
                        return;
                    }
                    else
                    {
                        callback((response.IsSuccessStatusCode, null, 0, 0, 0.0f));
                        return;
                    }
                }
                catch (HttpRequestException e)
                {
                    Plugin.Log.Error("EXCEPTION: " + e.ToString());
                    callback((false, null, 0, 0, 0.0f));
                }
            }
        }

        private string getLBDownloadJSON(string balls, int page, int limit)
        {
            var Data = new JObject
            {
                { "limit", limit },
                { "page", page },
                { "hash", balls },
                { "id", Plugin.userID },
            };
            return Data.ToString();
            return string.Empty;
        }

        public void GetBeatMapData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int, int, float)> callback)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => GetLeaderboardData(balls, page, callback));
        }


        public void SetBeatMapData(string balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods, Action<bool> callback)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => UploadLeaderboardData(balls, userID, username, badCuts, misses, fullCOmbo, acc, score, mods, callback));
        }

        private string getLBOverallJSON(string balls, string userID)
        {
            var Data = new JObject
            {
                { "Hash", balls },
                { "ID", userID },
            };
            return Data.ToString();
            return string.Empty;
        }


        private async Task UploadLeaderboardData(string balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods, Action<bool> callback)
        {

            using (var httpClient = new HttpClient())
            {
                int x = 0;
                while (x < 2)
                {
                    _panelView.prompt_loader.SetActive(true);
                    _panelView.promptText.gameObject.SetActive(true);
                    _panelView.promptText.text = "Uploading Score...";
                    try
                    {
                        _leaderboardView.userIDHere.text = userID;
                        var idBytes = Encoding.UTF8.GetBytes(userID);
                        var authKey = Convert.ToBase64String(idBytes);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authKey);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        string requestBody = getLBUploadJSON(balls, userID, username, badCuts, misses, fullCOmbo, acc, score, mods);

                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_UPLOAD_END_POINT, content);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            callback(response.IsSuccessStatusCode);
                            _panelView.prompt_loader.SetActive(false);
                            _panelView.promptText.text = "<color=green>Successfully uploaded score!</color>";
                            await Task.Delay(3000);
                            _panelView.promptText.gameObject.SetActive(false);
                            _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                            break;
                        }

                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=red>Failed to upload score... Retrying!</color>";
                        await Task.Delay(500);
                        _panelView.promptText.gameObject.SetActive(false);
                        _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                    }
                    catch (HttpRequestException e)
                    {
                        Plugin.Log.Error("EXCEPTION: " + e.ToString());
                        callback(false);
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=red>EXCEPTION ERROR</color>";
                        await Task.Delay(3000);
                        _panelView.promptText.gameObject.SetActive(false);
                    }
                    x++;
                }
                if (x == 2)
                {
                    callback(false);
                    _panelView.promptText.text = "<color=red>Failed to upload score... Retrying!</color>";
                    await Task.Delay(3000);
                    _panelView.promptText.gameObject.SetActive(false);
                    _panelView.prompt_loader.SetActive(false);
                }
            }
        }

        private string getLBUploadJSON(string balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods)
        {
            Plugin.Log.Info(username);
            var Data = new JObject
            {
                { "Hash", balls },
                { "UserID", userID },
                { "Username", username },
                { "BadCuts", badCuts },
                { "Misses", misses },
                { "FullCombo", fullCOmbo },
                { "Accuracy", acc },
                { "Score", score },
                { "Modifiers", mods },
            };
            return Data.ToString();
            return string.Empty;
        }

    }
}
