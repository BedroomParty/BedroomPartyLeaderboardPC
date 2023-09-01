using BedroomPartyLeaderboard.Installers;
using IPA;
using SiraUtil.Zenject;
using System.Net.Http;
using IPALogger = IPA.Logging.Logger;

namespace BedroomPartyLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        public static HttpClient httpClient { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            httpClient = new HttpClient();
            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}
