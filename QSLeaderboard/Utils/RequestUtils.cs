using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using QSLeaderboard.UI.Leaderboard;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using UnityEngine.Playables;
using Zenject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QSLeaderboard.Utils
{
    internal class RequestUtils
    {
        [Inject] private LeaderboardData _leaderboardData;
        [Inject] private LeaderboardView _leaderboardView;
        [Inject] private PanelView _panelView;

        private async Task GetLeaderboardData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>)> callback)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    Plugin.Log.Info("GETLEADERBOARDDATA");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    string requestBody = getLBDownloadJSON(balls, page, 10);


                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");


                    HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_DOWNLOAD_END_POINT, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Plugin.Log.Info($"{jsonResponse}");
                        var leaderboardData = _leaderboardData.LoadBeatMapInfo(jsonResponse);
                        callback((response.IsSuccessStatusCode, leaderboardData));
                        return;
                    }
                    else
                    {
                        Plugin.Log.Info("Map Not found");
                        callback((response.IsSuccessStatusCode, null));
                        return;
                    }
                }
                catch (HttpRequestException e)
                {
                    Plugin.Log.Error("EXCEPTION: " +  e.ToString());
                    callback((false, null));
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
            };
            return Data.ToString();
            return string.Empty;
        }

        public void GetBeatMapData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>)> callback)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => GetLeaderboardData(balls, page, callback));
        }


        public void SetBeatMapData(string balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods, Action<bool> callback)
        {
            Plugin.Log.Info("SetBeatMapData");
            UnityMainThreadTaskScheduler.Factory.StartNew(() => UploadLeaderboardData(balls, userID, username, badCuts, misses, fullCOmbo, acc, score, mods, callback));
        }


        private async Task UploadLeaderboardData(string balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods, Action<bool> callback)
        {
            Plugin.Log.Info("UploadLeaderboardData");
            _panelView.prompt_loader.SetActive(true);
            _panelView.promptText.gameObject.SetActive(true);
            _panelView.promptText.text = "Uploading Score...";
            using (var httpClient = new HttpClient())
            {
                try
                {
                    Plugin.Log.Info("UPLOAD LEADERBOARDDATA");
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
                        Plugin.Log.Info($"{jsonResponse}");
                        callback(response.IsSuccessStatusCode);
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=green>Successfully uploaded score!</color>";
                        await Task.Delay(3000);
                        _panelView.promptText.gameObject.SetActive(false);
                        _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                        return;
                    }
                    else
                    {
                        Plugin.Log.Info("Map Not found");
                        callback(response.IsSuccessStatusCode);
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=red>Failed to upload score</color>";
                        await Task.Delay(3000);
                        _panelView.promptText.gameObject.SetActive(false);
                        _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
                        return;
                    }
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
