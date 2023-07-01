using System.Linq;
using UnityEngine;

namespace QSLeaderboard.Utils
{
    public class Constants
    {
        public const string AUTH_END_POINT = "https://api.questsupporters.me/login";
        public const string LEADERBOARD_DOWNLOAD_END_POINT = "https://api.questsupporters.me/leaderboard";
        public const string LEADERBOARD_OVERALL_END_POINT = "https://api.questsupporters.me/leaderboard/overview";
        public const string LEADERBOARD_UPLOAD_END_POINT = "https://api.questsupporters.me/leaderboard/upload";
        public const string USER_URL = "https://api.questsupporters.me/user";
        public const string BALL_PATH = "./UserData/QSLeaderboard/Scary/";
        public const string PLAYLIST_PATH = "./Playlists/";

        public const string PLAYLIST_URL = "https://api.questsupporters.me/playlist/ranked";
        public const string USER_PROFILE_LINK = "https://questsupporters.me?user=";

        public const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";
        public static Color QS_COLOR = new Color(188f / 255f, 229f / 255f, 156f / 255f);
        public static string[] staffIDs = { "532063399069351947", "595628769138442250", "628480432467607552", "430459328852656148" };

        public static bool isStaff(string ballsack)
        {
            return staffIDs.Contains(ballsack);
        }
    }
}
