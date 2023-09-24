using BedroomPartyLeaderboard.AffinityPatches;
using Zenject;

namespace BedroomPartyLeaderboard.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ExtraSongData>().AsSingle();
        }
    }
}
