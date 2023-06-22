using QSLeaderboard.AffinityPatches;
using QSLeaderboard.UI.Leaderboard;
using QSLeaderboard.Utils;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace QSLeaderboard.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<LeaderboardUtils>().AsSingle();
            Container.Bind<PlayerUtils>().AsSingle();
            Container.Bind<RequestUtils>().AsSingle();
            Container.BindInterfacesTo<Results>().AsSingle();
            Container.Bind<LeaderboardData>().AsSingle();
            List<ImageHolder> holder = Enumerable.Range(0, 10).Select(x => new ImageHolder(x)).ToList();
            Container.Bind<List<ImageHolder>>().FromInstance(holder).AsSingle().WhenInjectedInto<LeaderboardView>();
        }
    }
}
