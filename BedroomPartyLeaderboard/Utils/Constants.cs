using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace BedroomPartyLeaderboard.Utils
{
    public class Constants
    {
        public const string AUTH_END_POINT = "https://api.thebedroom.party/user/login";

        public const string API_KEY_PATH = "./UserData/BPLB/scary/DO_NOT_SHARE.SCARY";

        public static string LEADERBOARD_DOWNLOAD_END_POINT(string hash)
        {
            return $"https://api.thebedroom.party/leaderboard/{hash}";
        }

        public static string LEADERBOARD_UPLOAD_END_POINT(string hash)
        {
            return $"https://api.thebedroom.party/leaderboard/{hash}/upload";
        }

        public const string USER_URL = "https://api.thebedroom.party/user";
        public const string PLAYLIST_PATH = "./Playlists/";

        public const string PLAYLIST_URL_RANKED = "https://api.thebedroom.party/playlist/ranked";
        public const string USER_PROFILE_LINK = "https://thebedroom.party?user=";

        public const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";

        public static Color BP_COLOR = new(0.674509804f, 0.760784314f, 0.850980392f);
        public static Color BP_COLOR2 = new(0.839215686f, 0.705882353f, 0.988235294f);

        public static string[] staffIDs = null;
        public const string BUG_REPORT_LINK = "https://thebedroom.party/?bugreports";

        public static async Task<bool> isStaff(string staffString)
        {
            using (HttpClient httpClient = new())
            {
                if (staffIDs == null)
                {
                    string a = await httpClient.GetStringAsync("https://api.thebedroom.party/staff");
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
