using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace QSLeaderboard.Utils
{
    internal class LeaderboardData
    {
        public struct LeaderboardEntry
        {
            public string userID;
            public string userName;
            public int missCount;
            public int badCutCount;
            public float acc;
            public bool fullCombo;
            public int score;
            public string mods;

            public LeaderboardEntry(string userID, string userName, int missCount, int badCutCount, float acc, bool fullCombo, int score, string mods)
            {
                this.userID = userID;
                this.userName = userName;
                this.missCount = missCount;
                this.badCutCount = badCutCount;
                this.acc = acc;
                this.fullCombo = fullCombo;
                this.score = score;
                this.mods = mods;
            }
        }

        public List<LeaderboardEntry> LoadBeatMapInfo(string json)
        {
            var leaderboard = new List<LeaderboardEntry>();

            JArray jsonArray = JArray.Parse(json);

            foreach (var scoreData in jsonArray)
            {
                Plugin.Log.Info("IM LOADING BEATMAP INFO");
                string userID = scoreData["UserID"]?.Value<string>();
                string userName = scoreData["Username"]?.Value<string>();
                int? missCount = scoreData["Misses"]?.Value<int>();
                int? badCutCount = scoreData["BadCuts"]?.Value<int>();
                float? acc = scoreData["Accuracy"]?.Value<float>();
                bool? fullCombo = scoreData["FullCombo"]?.Value<bool>();
                int? score = scoreData["Score"]?.Value<int>();
                string modifiers = scoreData["Modifiers"]?.Value<string>();

                Plugin.Log.Info($"userID - {userID}");
                Plugin.Log.Info($"userName - {userName}");
                Plugin.Log.Info($"missCount - {missCount}");
                Plugin.Log.Info($"badCutCount - {badCutCount}");
                Plugin.Log.Info($"acc - {acc}");
                Plugin.Log.Info($"fullCombo - {fullCombo}");
                Plugin.Log.Info($"score - {score}");
                Plugin.Log.Info($"modifiers - {modifiers}");

                leaderboard.Add(new LeaderboardEntry(
                    userID ?? "balls",
                    userName ?? "balls",
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
