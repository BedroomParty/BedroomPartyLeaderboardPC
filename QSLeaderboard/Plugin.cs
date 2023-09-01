using IPA;
using BedroomPartyLeaderboard.Installers;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BedroomPartyLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        public static bool Authed;

        public static string platformID;

        public static string discordID;

        public static string userName;
        public static string apiKey;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}
