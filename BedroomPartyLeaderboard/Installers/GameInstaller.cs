using BedroomPartyLeaderboard.AffinityPatches;
using BedroomPartyLeaderboard.Utils;
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
            ExtraSongDataHolder.reset();
            Container.BindInterfacesTo<ExtraSongData>().AsSingle();
        }
    }
}
