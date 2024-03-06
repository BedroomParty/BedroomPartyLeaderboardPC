using BloqParty.AffinityPatches;
using Zenject;

namespace BloqParty.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ExtraSongData>().AsSingle();
        }
    }
}
