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

            foreach (var scoreData in jsonArray)
            {
                Plugin.Log.Info("LOADBEATMAPINFO");
                int? rank = scoreData["Rank"]?.Value<int>();
                string userID = scoreData["UserID"]?.Value<string>();
                string userName = scoreData["Username"]?.Value<string>();
                float? PP = scoreData["PP"]?.Value<float>();
                int? missCount = scoreData["Misses"]?.Value<int>();
                int? badCutCount = scoreData["BadCuts"]?.Value<int>();
                float? acc = scoreData["Accuracy"]?.Value<float>();
                bool? fullCombo = scoreData["FullCombo"]?.Value<bool>();
                int? score = scoreData["Score"]?.Value<int>();
                string modifiers = scoreData["Modifiers"]?.Value<string>();

                leaderboard.Add(new LeaderboardEntry(
                    rank ?? 0,
                    userID ?? "balls",
                    userName ?? "balls",
                    PP ?? 0.0f,
                    missCount ?? 0,
                    badCutCount ?? 0,
                    acc ?? 0f,
                    fullCombo ?? false,
                    score ?? 0,
                    modifiers ?? ""
                ));
            }

            return leaderboard;
        }



    }
}
