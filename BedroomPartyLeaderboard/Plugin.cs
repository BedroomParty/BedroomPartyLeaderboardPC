using BedroomPartyLeaderboard.Installers;
using HarmonyLib;
using IPA;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace BedroomPartyLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        internal static Harmony harmony;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            harmony = new Harmony("Speecil.BeatSaber.BedroomPartyLeaderboard");
            zenjector.Install<MenuInstaller>(Location.Menu);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
