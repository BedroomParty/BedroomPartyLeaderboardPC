using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace QSLeaderboard.Utils
{
    internal class LeaderboardData
    {
        public struct LeaderboardEntry
        {
            public int rank;
            public string userID;
            public string userName;
            public float PP;
            public int missCount;
            public int badCutCount;
            public float acc;
            public bool fullCombo;
            public int score;
            public string mods;

            public LeaderboardEntry(int rank, string userID, string userName, float PP, int missCount, int badCutCount, float acc, bool fullCombo, int score, string mods)
            {
                this.rank = rank;
                this.userID = userID;
                this.userName = userName;
                this.PP = PP;
                this.missCount = missCount;
                this.badCutCount = badCutCount;
                this.acc = acc;
                this.fullCombo = fullCombo;
                this.score = score;
                this.mods = mods;
            }
        }

        public List<LeaderboardEntry> LoadBeatMapInfo(JArray jsonArray)
        {
            var leaderboard = new List<LeaderboardEntry>();
            Plugin.Log.Info("LOADBEATMAPINFO");
            foreach (var scoreData in jsonArray)
            {

                Plugin.Log.Info("score data in jason array");

                string rank = scoreData["rank"]?.ToString();
                string userID = scoreData["UserID"]?.ToString();
                string userName = scoreData["Username"]?.ToString();
                float PP = scoreData["PP"]?.Value<float>() ?? 0.0f;
                int missCount = scoreData["Misses"]?.Value<int>() ?? 0;
                int badCutCount = scoreData["BadCuts"]?.Value<int>() ?? 0;
                float acc = scoreData["Accuracy"]?.Value<float>() ?? 0.0f;
                bool fullCombo = scoreData["FullCombo"]?.Value<bool>() ?? false;
                int score = scoreData["Score"]?.Value<int>() ?? 0;
                string modifiers = scoreData["Modifiers"]?.ToString();

                leaderboard.Add(new LeaderboardEntry(
                    int.Parse(rank ?? "0"),
                    userID ?? "balls",
                    userName ?? "balls",
                    PP,
                    missCount,
                    badCutCount,
                    acc,
                    fullCombo,
                    score,
                    modifiers ?? ""
                ));
            }

            return leaderboard;
        }




    }
}
