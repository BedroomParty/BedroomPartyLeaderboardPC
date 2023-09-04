using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace BedroomPartyLeaderboard.Utils
{
    public class Constants
    {
        public const string AUTH_END_POINT = "https://api.thebedroom.party/user/login";

        public static string LEADERBOARD_DOWNLOAD_END_POINT(string hash) => $"https://api.thebedroom.party/leaderboard/{hash}";
        public static string LEADERBOARD_UPLOAD_END_POINT(string hash) => $"https://api.thebedroom.party/leaderboard/{hash}/upload";

        public const string USER_URL = "https://api.thebedroom.party/user";
        public const string PLAYLIST_PATH = "./Playlists/";

        public const string PLAYLIST_URL_RANKED = "https://api.thebedroom.party/playlist/ranked";
        public const string USER_PROFILE_LINK = "https://thebedroom.party?user=";

        public const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";

        public static Color BP_COLOR = new(123f / 255f, 39 / 255f, 81f / 255f);
        public static Color BP_COLOR2 = new(252f / 255, 208f / 255f, 185f / 255f);

        public static string[] staffIDs = null;
        public const string BUG_REPORT_LINK = "https://thebedroom.party/?bugreports";

        public static async Task<bool> isStaff(string uwu)
        {
            using (var httpClient = new HttpClient())
            if (staffIDs == null)
            {
                string a = await httpClient.GetStringAsync("https://api.thebedroom.party/staff");
                    staffIDs = a.Split(',');
            }
            return staffIDs.Contains(uwu); // we do not talk about it :clueless:
        }

        public static string profilePictureLink(string kms)
        {
            return $"https://cdn.phazed.xyz/QSBoard/High/{kms}.png";
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static List<LeaderboardData.LeaderboardEntry> EXAMPLEENTRIES = new();

        public static LeaderboardData.LeaderboardEntry GenerateRandomEntry(int position)
        {
            Random random = new();
            int rank = position; // Rank corresponds to position
            string userID = (position + 1).ToString(); // User ID as a number from 1 to 10
            string userName = "User" + random.Next(1, 100); // Generate a random user name
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // Current timestamp
            int missCount = random.Next(0, 50);
            int badCutCount = random.Next(0, 20);
            float acc = (float)(random.NextDouble() * 100.0); // Random accuracy between 0 and 100
            bool fullCombo = random.Next(0, 2) == 1; // Randomly true or false
            int score = random.Next(1000, 100000); // Assuming scores between 1000 and 100000
            string mods = "RandomMods"; // You can replace this with a random mods generator

            return new LeaderboardData.LeaderboardEntry(rank, userID, userName, timestamp, missCount, badCutCount, acc, fullCombo, score, mods);
        }
    }
}
