using Newtonsoft.Json;
using System.Collections.Generic;

namespace BedroomPartyLeaderboard.Utils
{
    public class LeaderboardData
    {
        internal class BPLeaderboard
        {
            [JsonProperty("scoreCount")] public int scoreCount;
            [JsonProperty("scores")] public List<LeaderboardEntry> scores;
        }

        internal class LeaderboardEntry
        {
            [JsonProperty("rank")] public int rank;
            [JsonProperty("id")] public string userID;
            [JsonProperty("username")] public string userName;
            [JsonProperty("timeSet")] public long timestamp;
            [JsonProperty("misses")] public int missCount;
            [JsonProperty("badCuts")] public int badCutCount;
            [JsonProperty("accuracy")] public float acc;
            [JsonProperty("fullCombo")] public bool fullCombo;
            [JsonProperty("modifiedScore")] public int modifiedScore;
            [JsonProperty("multipliedScore")] public int multipliedScore;
            [JsonProperty("modifiers")] public string mods;
        }
    }
}
