﻿using IPA.Loader;
using Newtonsoft.Json;
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
                authKey = sillyvar[0];
                playerId = sillyvar[1];
            }
            else
            {
                taskCompletionSource.SetResult(new PlayerInfo(playerName, playerId, null, "", "", 0));
                return taskCompletionSource.Task;
            }

            if (playerId == "" || authKey == "")
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
            JObject user = new()
            {
                { "id", userID },
                { "gameVersion", "v" + IPA.Utilities.UnityGame.GameVersion.StringValue.Split('_')[0] },
                { "pluginVersion", "PC v" + PluginManager.GetPlugin("BedroomPartyLeaderboard").HVersion.ToString() }
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
        [JsonProperty("sessionKey")] public string sessionKey;
        [JsonProperty("sessionKeyExpires")] public long sessionKeyExpires;
    }
}
