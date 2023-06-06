using IPA.Utilities.Async;
using QSLeaderboard.UI.Leaderboard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Zenject;

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
                var url = $"http://168.138.9.99:5000/api/leaderboard?hash={balls}&page={page}";
                try
                {
                    Plugin.Log.Info("GETLEADERBOARDDATA");
                    var response = await httpClient.GetAsync(url);

                    bool tooFar = response.StatusCode == System.Net.HttpStatusCode.Gone;
                    bool isSuccessful = response.StatusCode == System.Net.HttpStatusCode.OK;

                    if (tooFar)
                    {
                        _leaderboardView.OnPageUp(tooFar);
                        callback((false, null));
                        return;
                    }


                    if (isSuccessful)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Plugin.Log.Info($"{jsonResponse}");
                        var leaderboardData = _leaderboardData.LoadBeatMapInfo(jsonResponse);
                        callback((isSuccessful, leaderboardData));
                        return;
                    }
                    else
                    {
                        Plugin.Log.Info("Map Not found");
                        callback((isSuccessful, null));
                        return;
                    }
                }
                catch (HttpRequestException)
                {
                    Plugin.Log.Error("EXCEPTION");
                    callback((false, null));
                }
            }
        }

        public void GetBeatMapData(string balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>)> callback)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => GetLeaderboardData(balls, page, callback));
        }


        public void PushLBData(string balls, string userID, string username, int badCuts, int misses, bool fullCOmbo, float acc, int score, string mods)
        {
            Plugin.Log.Info("PushLBData");
            _panelView.promptText.text = "Saving...";
            _panelView.promptText.gameObject.SetActive(true);
            _panelView.prompt_loader.SetActive(true);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://url");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    hash = balls,
                    UserID = userID,
                    Username = username,
                    BadCuts = badCuts,
                    Misses = misses,
                    FullCombo = fullCOmbo,
                    Accuracy = acc,
                    Score = score,
                    Modifiers = mods
                });

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            
            if(httpResponse.StatusCode == HttpStatusCode.OK)
            {
                _panelView.promptText.text = "<color=green>Successfully saved data!</color>";
                _panelView.promptText.gameObject.SetActive(false);
                _panelView.prompt_loader.SetActive(false);
                Plugin.Log.Info("200 Response on post");
            }
            else
            {
                _panelView.promptText.text = "<color=red>Failed to save data!</color>";
                _panelView.prompt_loader.SetActive(false);
                _panelView.promptText.gameObject.SetActive(false);
                Plugin.Log.Info("410 Response on post");
            }

        }

    }
}
