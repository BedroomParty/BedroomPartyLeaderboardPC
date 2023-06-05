using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using QSLeaderboard.UI.SettingsMenu;
using QSLeaderboard.Installers;

namespace QSLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            SettingsMenu.instance.Init();
            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}
