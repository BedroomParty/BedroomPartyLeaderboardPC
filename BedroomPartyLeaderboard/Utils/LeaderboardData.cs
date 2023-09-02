using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BedroomPartyLeaderboard.Utils
{
    public class LeaderboardData
    {
        public struct LeaderboardEntry
        {
            public int rank;
            public string userID;
            public string userName;
            public long timestamp;
            public int missCount;
            public int badCutCount;
            public float acc;
            public bool fullCombo;
            public int score;
            public string mods;

            public LeaderboardEntry(int rank, string userID, string userName, long timestamp, int missCount, int badCutCount, float acc, bool fullCombo, int score, string mods)
            {
                this.rank = rank;
                this.userID = userID;
                this.userName = userName;
                this.timestamp = timestamp;
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
                string rank = scoreData["Rank"]?.ToString();
                string userID = scoreData["UserID"]?.ToString();
                string userName = scoreData["Username"]?.ToString();
                int missCount = scoreData["misses"]?.Value<int>() ?? 0;
                int badCutCount = scoreData["badCuts"]?.Value<int>() ?? 0;
                float acc = scoreData["accuracy"]?.Value<float>() ?? 0.0f;
                bool fullCombo = scoreData["fullCombo"]?.Value<bool>() ?? false;
                int score = scoreData["score"]?.Value<int>() ?? 0;
                string modifiers = scoreData["modifiers"]?.ToString();
                long timestamp = scoreData["TimeSet"]?.Value<long>() ?? 0;
                leaderboard.Add(new LeaderboardEntry(
                    int.Parse(rank ?? "0"),
                    userID ?? "0",
                    userName ?? "Player",
                    timestamp,
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
