using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using QSLeaderboard.Installers;

namespace QSLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        public static bool Authed;
        public static string userID;
        public static string userName;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}
