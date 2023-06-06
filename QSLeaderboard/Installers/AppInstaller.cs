using QSLeaderboard.AffinityPatches;
using QSLeaderboard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace QSLeaderboard.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<RequestUtils>().AsSingle();
            Container.BindInterfacesTo<Results>().AsSingle();
        }
    }
}
