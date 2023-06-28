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
        private async Task GetLeaderboardData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int, int, float, float)> callback)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    string requestString = getLBDownloadJSON(balls, page, 10, "top");

                    HttpResponseMessage response = await httpClient.GetAsync(requestString);

                    int rank = 0;
                    float pp = 0f;
                    int scorecount = 0;
                    float stars = 0;
                    int totalPages = 0;
                    List<LeaderboardData.LeaderboardEntry> data = new List<LeaderboardData.LeaderboardEntry>();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Plugin.Log.Info(jsonResponse.ToString());
                        JObject jsonObject = JObject.Parse(jsonResponse);

                        if (jsonObject.TryGetValue("GlobalRank", out JToken globalRankToken))
                            rank = globalRankToken.Value<int>();
                        else
                            rank = 0;

                        if (jsonObject.TryGetValue("PP", out JToken PP))
                            pp = PP.Value<float>();
                        else
                            pp = 0;

                        if (jsonObject.TryGetValue("ScoreCount", out JToken scoreCountToken))
                            scorecount = scoreCountToken.Value<int>();
                        else
                            scorecount = 0;

                        if (jsonObject.TryGetValue("Stars", out JToken starsToken))
                            stars = starsToken.Value<float>();
                        else
                            stars = 0f;

                        totalPages = Mathf.CeilToInt((float)scorecount / 10);

                        if (jsonObject.TryGetValue("Scores", out JToken scoresToken) && scoresToken is JArray scoresArray && scoresArray.Count > 0)
                            data = _leaderboardData.LoadBeatMapInfo(scoresArray);
                        else
                            data = new List<LeaderboardData.LeaderboardEntry>();

                    }

                    callback((response.IsSuccessStatusCode, data, rank, totalPages, stars, pp));
                    return;
                }
                catch (HttpRequestException e)
                {
                    Plugin.Log.Error("EXCEPTION: " + e.ToString());
                    callback((false, null, 0, 0, 0f, 0f));
                }
            }
        }

        private string getLBDownloadJSON(string balls, int page, int limit, string sort)
        {
            var Data = $"{Constants.LEADERBOARD_DOWNLOAD_END_POINT}/{balls}?sort={sort}&limit=10&page={page}&user={Plugin.userID}";
            return Data;
        }

        public void GetBeatMapData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int, int, float, float)> callback)
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
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Plugin.apiKey);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        string requestBody = getLBUploadJSON(balls, userID, username, badCuts, misses, fullCOmbo, acc, score, mods);

                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_UPLOAD_END_POINT, content);

                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            callback(response.IsSuccessStatusCode);
                            _panelView.prompt_loader.SetActive(false);
                            _panelView.promptText.text = "<color=red>Better score already exists.</color>";
                            await Task.Delay(3000);
                            _panelView.promptText.gameObject.SetActive(false);
                            _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                            break;
                        }
                        else if (response.StatusCode == HttpStatusCode.OK)
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
