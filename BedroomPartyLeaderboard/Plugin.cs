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

        internal static HttpClient httpClient { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            zenjector.Install<MenuInstaller>(Location.Menu);
            httpClient = new HttpClient();
        }
    }
}
