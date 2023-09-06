using Newtonsoft.Json;

namespace BedroomPartyLeaderboard.Utils
{
    internal class ExternalPlayerInfo
    {
        internal class PlayerInfo
        {
            [JsonProperty("discord_id")] public long discordID;
            [JsonProperty("game_id")] public long gameID;
            [JsonProperty("username")] public string username;
            [JsonProperty("avatar")] public string avatarLink;
        }
    }
}
