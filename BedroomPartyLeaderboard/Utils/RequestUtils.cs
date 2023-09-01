using BedroomPartyLeaderboard.UI.Leaderboard;
using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class RequestUtils
    {
        [Inject] private LeaderboardData _leaderboardData;
        [Inject] private LeaderboardView _leaderboardView;
        [Inject] private PanelView _panelView;
        [Inject] private readonly PlayerUtils _playerUtils;
        private async Task GetLeaderboardData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int, int, float, float)> callback)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    string requestString = getLBDownloadJSON(balls, page, 10, _leaderboardView.sortMethod);

                    HttpResponseMessage response = await httpClient.GetAsync(requestString);

                    int rank = 0;
                    float pp = 0f;
                    int scorecount = 0;
                    float stars = 0;
                    int totalPages = 0;
                    List<LeaderboardData.LeaderboardEntry> data = new List<LeaderboardData.LeaderboardEntry>();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(jsonResponse);

                    if (jsonObject.TryGetValue("GlobalRank", out JToken globalRankToken)) rank = globalRankToken.Value<int>();
                    else rank = 0;

                    if (jsonObject.TryGetValue("PP", out JToken PP)) pp = PP.Value<float>();
                    else pp = 0;

                    if (jsonObject.TryGetValue("ScoreCount", out JToken scoreCountToken)) scorecount = scoreCountToken.Value<int>();
                    else scorecount = 0;

                    if (jsonObject.TryGetValue("Stars", out JToken starsToken)) stars = starsToken.Value<float>();
                    else stars = 0f;

                    totalPages = Mathf.CeilToInt((float)scorecount / 10);

                    if (jsonObject.TryGetValue("Scores", out JToken scoresToken) && scoresToken is JArray scoresArray && scoresArray.Count > 0) data = _leaderboardData.LoadBeatMapInfo(scoresArray);
                    else data = new List<LeaderboardData.LeaderboardEntry>();

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
            var Data = $"{Constants.LEADERBOARD_DOWNLOAD_END_POINT}/{balls}?sort={sort}&limit=10&page={page}&id={_playerUtils.localPlayerInfo.userID}";
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
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _playerUtils.localPlayerInfo.authKey);
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
        }
        public void FUCKOFFPLAYLIST()
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => FUCK());
        }
        private async Task FUCK()
        {
            using (var httpClient = new HttpClient())
            {
                int x = 0;
                while (x < 2)
                {
                    _panelView.prompt_loader.SetActive(true);
                    _panelView.promptText.gameObject.SetActive(true);
                    _leaderboardView.playlistButton.interactable = false;
                    _panelView.promptText.text = "Downloading Playlist...";
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(Constants.PLAYLIST_URL_RANKED);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var playlistStringLMFAOOOOO = await response.Content.ReadAsStringAsync();


                            if (!string.IsNullOrEmpty(playlistStringLMFAOOOOO))
                            {
                                if (!File.Exists(Constants.PLAYLIST_PATH + "QS-Ranked.bplist"))
                                {
                                    using (File.Create(Constants.PLAYLIST_PATH + "QS-Ranked.bplist")) { }
                                }
                                string apiKeyFilePath = Constants.PLAYLIST_PATH + "QS-Ranked.bplist";

                                using (StreamWriter sw = new(apiKeyFilePath))
                                {
                                    await sw.WriteAsync(playlistStringLMFAOOOOO);
                                }
                                TryRefreshPlaylists(true);
                            }
                            else
                            {
                                Plugin.Log.Error("Failed to parse API key from the response.");
                            }
                        }
                        else
                        {
                            Plugin.Log.Error("Failed to download the ranked playlist");
                        }

                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=green>Successfully downloaded the ranked playlist!</color>";
                        await Task.Delay(3000);
                        _panelView.promptText.gameObject.SetActive(false);
                        _leaderboardView.playlistButton.interactable = true;
                        break;
                    }
                    catch (HttpListenerException e)
                    {
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=red>Failed to download the playlist... Retrying!</color>";
                        await Task.Delay(500);
                        _panelView.promptText.gameObject.SetActive(false);
                        _leaderboardView.playlistButton.interactable = true;
                    }
                    catch (HttpRequestException e)
                    {
                        Plugin.Log.Error("EXCEPTION: " + e.ToString());
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = "<color=red>EXCEPTION ERROR</color>";
                        await Task.Delay(3000);
                        _panelView.promptText.gameObject.SetActive(false);
                        _leaderboardView.playlistButton.interactable = true;
                        return;
                    }
                    x++;
                }
                if (x == 2)
                {
                    _panelView.promptText.text = "<color=red>Failed to download the playlist... Retrying!</color>";
                    await Task.Delay(3000);
                    _panelView.promptText.gameObject.SetActive(false);
                    _panelView.prompt_loader.SetActive(false);
                    _leaderboardView.playlistButton.interactable = true;
                }
            }
        }

        public static bool TryRefreshPlaylists(bool fullRefresh)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BeatSaberPlaylistsLib");
                var playlistManagerType = assembly!.GetType("BeatSaberPlaylistsLib.PlaylistManager");
                var defaultManagerField = playlistManagerType.GetProperty("DefaultManager", BindingFlags.Static | BindingFlags.Public);
                var refreshMethodInfo = playlistManagerType.GetMethod("RefreshPlaylists", BindingFlags.Instance | BindingFlags.Public);

                var manager = defaultManagerField!.GetValue(null);
                refreshMethodInfo!.Invoke(manager, new object[] { fullRefresh });
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.Debug($"RefreshPlaylists failed: {e}");
                return false;
            }
        }
    }
}
