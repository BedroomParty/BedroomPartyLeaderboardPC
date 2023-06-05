using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeaderboardTableView;

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

        public  List<LeaderboardEntry> LoadBeatMapInfo(string json)
        {
            var leaderboard = new List<LeaderboardEntry>();

            var jsonObject = JObject.Parse(json);

            foreach (var scoreData in jsonObject)
            {
                Plugin.Log.Info("IM LOADING BEATMAP INFO");
                string userID = scoreData.Value["userID"]?.Value<string>();
                string userName = scoreData.Value["username"]?.Value<string>();
                int? missCount = scoreData.Value["misses"]?.Value<int>();
                int? badCutCount = scoreData.Value["badCuts"]?.Value<int>();
                float? acc = scoreData.Value["accuracy"]?.Value<float>();
                bool? fullCombo = scoreData.Value["fullCombo"]?.Value<bool>();
                int? score = scoreData.Value["score"]?.Value<int>();
                string modifiers = scoreData.Value["modifiers"]?.Value<string>();

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


        public List<ScoreData> CreateLeaderboardData(List<LeaderboardEntry> leaderboard, int page)
        {
            List<ScoreData> tableData = new List<ScoreData>();
            int pageIndex = page * 10;
            for (int i = pageIndex; i < leaderboard.Count && i < pageIndex + 10; i++)
            {
                Plugin.Log.Notice("Creating LB DATA");
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + 1, score));
            }
            return tableData;
        }

        public ScoreData CreateLeaderboardEntryData(LeaderboardEntry entry, int rank, int score)
        {
            string formattedUsername = $"{entry.userName}";

            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            score = entry.score;
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);

            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;

            result = "<size=100%>" + formattedUsername + formattedAcc + formattedCombo + formattedMods + "</size>";
            return new ScoreData(score, result, rank, false);
        }
    }
}
