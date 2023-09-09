using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BedroomPartyLeaderboard.Utils
{
    internal class PlayerUtils
    {
        public Task<PlayerInfo> GetPlayerInfoAsync()
        {
            TaskCompletionSource<PlayerInfo> taskCompletionSource = new();
            string playerId = "";
            string playerName = "";
            string authKey = "";

            if (NullCheckFilePath(Constants.API_KEY_PATH))
            {
                string[] sillyvar = Constants.Base64Decode(File.ReadAllText(Constants.API_KEY_PATH)).Split(',');
                playerId = sillyvar[1];
                authKey = sillyvar[0];
            }
            else
            {
                taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, ""));
                return taskCompletionSource.Task;
            }

            if (playerId == "" || authKey == "")
            {
                taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, ""));
                return taskCompletionSource.Task;
            }

            taskCompletionSource.SetResult(new PlayerInfo("", playerId, authKey, ""));
            return taskCompletionSource.Task;
        }

        private bool NullCheckFilePath(string path)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
                return false;
            }
            return true;
        }

        public string GetLoginString(string userID)
        {
            JObject user = new()
            {
                { "id", userID },
            };

            return user.ToString();
        }

        public struct PlayerInfo
        {
            public string username;
            public string userID;
            public readonly string authKey;
            internal string tempKey;

            public PlayerInfo(string username, string userID, string authKey, string tempKey)
            {
                this.authKey = authKey;
                this.username = username;
                this.userID = userID;
                this.tempKey = tempKey;
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
}
