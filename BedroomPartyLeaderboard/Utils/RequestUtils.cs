using BedroomPartyLeaderboard.UI.Leaderboard;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        [Inject] private readonly PlayerUtils _playerUtils;
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
            string Data = $"{Constants.LEADERBOARD_DOWNLOAD_END_POINT(balls.Item1)}?char={balls.Item3}&diff={balls.Item2}&sort={sort}&limit=10&page={page}&id={_playerUtils.localPlayerInfo.userID}";
            return Data;
        }

        public void GetBeatMapData((string, int, string) balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int)> callback)
        {
            _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => GetLeaderboardData(balls, page, callback));
        }

        public void SetBeatMapData((string, int, string) balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods, int multipliedScore, int modifiedScore, Action<bool> callback)
        {
            _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => UploadLeaderboardData(balls, userID, username, badCuts, misses, fullCOmbo, acc, score, mods, callback, multipliedScore, modifiedScore));
        }

        private async Task UploadLeaderboardData((string, int, string) balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods, Action<bool> callback, int multipliedScore, int modifiedScore)
        {
            using HttpClient httpClient = new();
            int x = 0;
            while (x < 2)
            {
                _panelView.prompt_loader.SetActive(true);
                _panelView.promptText.gameObject.SetActive(true);
                _panelView.promptText.text = "Uploading Score...";
                try
                {
                    _ = httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _playerUtils.localPlayerInfo.authKey);
                    _ = httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    string requestBody = getLBUploadJSON(balls, userID, badCuts, misses, fullCOmbo, acc, mods, modifiedScore, multipliedScore);

                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_UPLOAD_END_POINT(balls.Item1), content);

                    if (response.StatusCode == HttpStatusCode.Conflict)
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
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        callback(response.IsSuccessStatusCode);
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=green>Successfully uploaded score!</color>";
                        _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                        await Task.Delay(3000);
                        _panelView.promptText.gameObject.SetActive(false);
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

        private string getLBUploadJSON((string, int, string) balls, string userID, int badCuts, int misses, bool fullCOmbo, float acc, string mods, int modifiedScore, int multipliedScore)
        {
            JObject Data = new()
            {
                { "hash", balls.Item1 },
                { "difficulty", balls.Item2 },
                { "characteristic", balls.Item3 },
                { "id", userID },
                { "badCuts", badCuts },
                { "misses", misses },
                { "fullCombo", fullCOmbo },
                { "accuracy", acc },
                { "modifiedScore", modifiedScore },
                { "multipliedScore", multipliedScore },
                { "modifiers", mods },
            };
            return Data.ToString();
        }
    }
}
