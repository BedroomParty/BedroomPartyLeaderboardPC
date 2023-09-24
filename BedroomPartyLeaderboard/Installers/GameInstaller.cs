using BedroomPartyLeaderboard.AffinityPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
