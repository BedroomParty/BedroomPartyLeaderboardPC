using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace BedroomPartyLeaderboard.Utils
{
    public class Constants
    {
        public const string BASE_API_URL = "https://api.thebedroom.party";
        public const string BASE_WEB_URL = "https://thebedroom.party";


        public const string AUTH_END_POINT = BASE_API_URL + "/user/login";
        public const string API_KEY_PATH = "./UserData/BPLB/scary/DO_NOT_SHARE.SCARY";
        public const string PLAYLIST_PATH = "./Playlists/";
        public const string PLAYLIST_URL_RANKED = BASE_API_URL + "/playlist/ranked";
        public const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";

        public static string LEADERBOARD_DOWNLOAD_END_POINT(string hash) => $"{BASE_API_URL}/leaderboard/{hash}";
        public static string LEADERBOARD_UPLOAD_END_POINT(string hash) => $"{BASE_API_URL}/leaderboard/{hash}/upload";
        public static string USER_URL_WEB(string userID) => $"{BASE_WEB_URL}/user/{userID}";
        public static string USER_URL_API(string userID) => $"{BASE_API_URL}/user/{userID}";


        public static Color BP_COLOR = new(0.674509804f, 0.760784314f, 0.850980392f);
        public static Color BP_COLOR2 = new(0.839215686f, 0.705882353f, 0.988235294f);

        public static string[] staffIDs = null;
        public const string BUG_REPORT_LINK = BASE_WEB_URL + "/bug-report";

        public static async Task<bool> isStaff(string staffString)
        {
            using (HttpClient httpClient = new())
            {
                if (staffIDs == null)
                {
                    string a = await httpClient.GetStringAsync($"{BASE_API_URL}/staff");
                    staffIDs = a.Split(',');
                }
            }

            return staffIDs.Contains(staffString);
        }

        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string plainText)
        {
            byte[] plainTextBytes = System.Convert.FromBase64String(plainText);
            return System.Text.Encoding.UTF8.GetString(plainTextBytes);
        }

        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            Task waitTask = Task.Run(async () =>
            {
                while (!condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }
    }
}
