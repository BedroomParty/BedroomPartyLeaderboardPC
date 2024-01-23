using BedroomPartyLeaderboard.Installers;
using IPA;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BedroomPartyLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        public static bool isDev = true;

        [Init]
        public Plugin(Zenjector zenjector)
        {
            zenjector.UseLogger(Log);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.GameCore);
        }
    }
}
