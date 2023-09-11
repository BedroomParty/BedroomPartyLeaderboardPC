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

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.StandardPlayer);
        }
    }
}
