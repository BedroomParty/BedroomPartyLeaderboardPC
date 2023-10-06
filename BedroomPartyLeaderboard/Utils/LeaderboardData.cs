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
            [JsonProperty("rank")] public int? rank = null;
            [JsonProperty("id")] public string? userID = null;
            [JsonProperty("username")] public string? userName = null;
            [JsonProperty("timeSet")] public long? timestamp = null;
            [JsonProperty("misses")] public int? missCount = null;
            [JsonProperty("badCuts")] public int? badCutCount = null;
            [JsonProperty("accuracy")] public float? acc = null;
            [JsonProperty("fullCombo")] public bool? fullCombo = null;
            [JsonProperty("modifiedScore")] public int? modifiedScore = null;
            [JsonProperty("multipliedScore")] public int? multipliedScore = null;
            [JsonProperty("modifiers")] public string? mods = null;
            [JsonProperty("pauses")] public int? pauses = null;
            [JsonProperty("maxCombo")] public int? maxCombo = null;
            [JsonProperty("rightHandTimeDependency")] public float? rightHandTimeDependency = null;
            [JsonProperty("leftHandTimeDependency")] public float? leftHandTimeDependency = null;
            [JsonProperty("rightHandAverageScore")] public float? rightHandAverageScore = null;
            [JsonProperty("leftHandAverageScore")] public float? leftHandAverageScore = null;
            [JsonProperty("perfectStreak")] public int? perfectStreak = null;
            [JsonProperty("fcAccuracy")] public float? fcAcc = null;
        }
    }
}
