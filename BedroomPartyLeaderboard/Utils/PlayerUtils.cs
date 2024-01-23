using IPA.Loader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class PlayerUtils
    {
        [Inject] private HeadsetUtils headsetUtils;

        public Task<PlayerInfo> GetPlayerInfoAsync()
        {
            TaskCompletionSource<PlayerInfo> taskCompletionSource = new TaskCompletionSource<PlayerInfo>();

            string playerId = "";
            string playerName = "";

            string authKey;

            playerId = "";
            playerName = "";
            if (NullCheckFilePath(Constants.API_KEY_PATH))
            {
                try
                {
                    string[] keyData = Constants.Base64Decode(File.ReadAllText(Constants.API_KEY_PATH)).Split(',');

                    authKey = keyData[0];
                    playerId = keyData[1];
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, "", "", 0));
                    return taskCompletionSource.Task;
                }
            }
            else
            {
                taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, "", "", 0));
                return taskCompletionSource.Task;
            }

            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(authKey))
            {
                taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, "", "", 0));
                return taskCompletionSource.Task;
            }

            taskCompletionSource.SetResult(new PlayerInfo("", playerId, authKey, "", "", 0));
            return taskCompletionSource.Task;
        }


        private bool NullCheckFilePath(string path)
        {
            if (path == null) return false;

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                return false;
            }
            return true;
        }

        private bool NullCheckDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return false;
            }
            return true;
        }

        public string GetLoginString(string userID)
        {
            headsetUtils.GetHMDInfo(out string hmd, out HeadsetUtils.HMD hmd1);
            JObject user = new()
            {
                { "id", userID },
                { "gameVersion", "v" + IPA.Utilities.UnityGame.GameVersion.StringValue.Split('_')[0] },
                { "pluginVersion", "PC v" + PluginManager.GetPlugin("BedroomPartyLeaderboard").HVersion.ToString() },
                { "hmd",  (int)hmd1 }
            };
            return user.ToString();
        }

        public struct PlayerInfo
        {
            internal string username;
            internal string userID;
            internal readonly string authKey;
            internal string tempKey;
            internal string discordID;
            internal long sessionExpiry;

            public PlayerInfo(string username, string userID, string authKey, string tempKey, string discordID, long sessionExpiry)
            {
                this.authKey = authKey;
                this.username = username;
                this.userID = userID;
                this.tempKey = tempKey;
                this.discordID = discordID;
                this.sessionExpiry = sessionExpiry;
            }

            public PlayerInfoAwaiter GetAwaiter()
            {
                return new PlayerInfoAwaiter(this);
            }

            public struct PlayerInfoAwaiter : INotifyCompletion
            {
                private readonly PlayerInfo _playerInfo;

                public PlayerInfoAwaiter(PlayerInfo playerInfo)
                {
                    _playerInfo = playerInfo;
                }

                public bool IsCompleted => true;

                public PlayerInfo GetResult()
                {
                    return _playerInfo;
                }

                public void OnCompleted(Action continuation)
                {
                    continuation();
                }
            }
        }
    }

    public class PlayerResponse
    {
        [JsonProperty("gameID")] public string gameID;
        [JsonProperty("discordID")] public string discordID;
        [JsonProperty("username")] public string username;
        [JsonProperty("avatar")] public string avatarLink;
        [JsonProperty("description")] public string userDescription;
        [JsonProperty("sessionKey")] public string sessionKey;
        [JsonProperty("sessionKeyExpires")] public long sessionKeyExpires;
        [JsonProperty("updateAvailable")] public bool updateAvailable;
    }
}