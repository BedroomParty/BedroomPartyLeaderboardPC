using System.Linq;
using UnityEngine;

namespace BedroomPartyLeaderboard.Utils
{
    public class Constants
    {
        public const string AUTH_END_POINT = "https://api.thebedroom.party/login";

        public const string LEADERBOARD_DOWNLOAD_END_POINT = "https://api.thebedroom.party/leaderboard";
        public const string LEADERBOARD_OVERALL_END_POINT = "https://api.thebedroom.party/leaderboard/overview";
        public const string LEADERBOARD_UPLOAD_END_POINT = "https://api.thebedroom.party/leaderboard/upload";

        public const string USER_URL = "https://api.thebedroom.party/user";
        public const string PLAYLIST_PATH = "./Playlists/";

        public const string PLAYLIST_URL_RANKED = "https://api.thebedroom.party/playlist/ranked";
        public const string USER_PROFILE_LINK = "https://thebedroom.party?user=";

        public const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";

        public static Color BP_COLOR = new Color(123f / 255f, 39 / 255f, 81f / 255f);
        public static Color BP_COLOR2 = new Color(252f / 255, 208f / 255f, 185f / 255f);

        public static string[] staffIDs;
        public const string BUG_REPORT_LINK = "https://thebedroom.party/?bugreports";

        public static bool isStaff(string uwu)
        {
            if (staffIDs == null)
            {
                // begin fetch of staffIDs
            }
            return staffIDs.Contains(uwu); // we do not talk about it :clueless:
        }

        public static string profilePictureLink(string kms)
        {
            return $"https://cdn.phazed.xyz/QSBoard/High/{kms}.png";
        }
    }
}
