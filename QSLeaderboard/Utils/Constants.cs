using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSLeaderboard.Utils
{
    public class Constants
    {
        public const string AUTH_END_POINT = "http://168.138.9.99:5000/api/login";
        public const string LEADERBOARD_DOWNLOAD_END_POINT = "http://168.138.9.99:5000/api/leaderboard/scores";
        public const string LEADERBOARD_UPLOAD_END_POINT = "http://168.138.9.99:5000/api/leaderboard/upload";

        public const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";
        public static Color QS_COLOR = new Color(188f / 255f, 229f / 255f, 156f / 255f);
    }
}
