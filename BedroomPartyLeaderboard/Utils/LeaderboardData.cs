using BedroomPartyLeaderboard.UI.Leaderboard;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    public class LeaderboardData
    {
        [Inject] private LeaderboardView _leaderboardView;
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
            int i = 0;
            foreach (var scoreData in jsonArray)
            {
                int rank = ((_leaderboardView.page * 10) - (10 - i)) + 1;
                int userID = scoreData["id"]?.Value<int>() ?? 0;
                string userName = scoreData["username"]?.ToString();
                int missCount = scoreData["misses"]?.Value<int>() ?? 0;
                int badCutCount = scoreData["badCuts"]?.Value<int>() ?? 0;
                float acc = scoreData["accuracy"]?.Value<float>() ?? 0.0f;
                bool fullCombo = scoreData["fullCombo"]?.Value<bool>() ?? false;
                int score = scoreData["score"]?.Value<int>() ?? 0;
                string modifiers = scoreData["modifiers"]?.ToString();
                long timestamp = scoreData["timeSet"]?.Value<long>() ?? 0;
                leaderboard.Add(new LeaderboardEntry(
                    rank,
                    userID.ToString(),
                    userName ?? "Player",
                    timestamp,
                    missCount,
                    badCutCount,
                    acc,
                    fullCombo,
                    score,
                    modifiers ?? ""
                ));
                i++;
            }
            return leaderboard;
        }
    }
}
