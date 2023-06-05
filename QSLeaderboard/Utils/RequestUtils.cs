using QSLeaderboard.UI.Leaderboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace QSLeaderboard.Utils
{
    internal class RequestUtils
    {
        [Inject] private LeaderboardData _leaderboardData;
        private async Task GetLeaderboardData(string balls, Action<(bool, List<LeaderboardData.LeaderboardEntry>)> callback)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"http://168.138.9.99:5000/api/leaderboard?hash={balls}";
                try
                {
                    Plugin.Log.Info("GETLEADERBOARDDATA");
                    var response = await httpClient.GetAsync(url);
                    bool isSuccessful = response.StatusCode == System.Net.HttpStatusCode.OK;

                    if (isSuccessful)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Plugin.Log.Info($"{jsonResponse}"); 
                        var leaderboardData = _leaderboardData.LoadBeatMapInfo(jsonResponse);
                        callback((isSuccessful, leaderboardData));
                    }
                    else
                    {
                        Plugin.Log.Info("error");
                        callback((isSuccessful, default));
                    }
                }
                catch (HttpRequestException)
                {
                    Plugin.Log.Error("EXCEPTION");
                    callback((false, default));
                }
            }
        }

        public void GetBeatMapData(string balls, Action<(bool, List<LeaderboardData.LeaderboardEntry>)> callback)
        {
            Task.Run(() => GetLeaderboardData(balls, callback));
        }
    }
}
