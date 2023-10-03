using Newtonsoft.Json;

namespace BedroomPartyLeaderboard.Utils
{
    internal class ExternalPlayerInfo
    {
        internal class PlayerInfo
        {
            [JsonProperty("discordID")] public long discordID;
            [JsonProperty("gameID")] public long gameID;
            [JsonProperty("username")] public string username;
            [JsonProperty("avatar")] public string avatarLink;
        }
    }
}
