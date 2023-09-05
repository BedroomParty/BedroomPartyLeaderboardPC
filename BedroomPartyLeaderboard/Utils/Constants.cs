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

        public const string API_KEY_PATH = "./UserData/BPLB/scary/DO_NOT_SHARE.SCARY";

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

    }
}
