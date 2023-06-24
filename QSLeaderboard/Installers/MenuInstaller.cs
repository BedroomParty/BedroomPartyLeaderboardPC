using QSLeaderboard.AffinityPatches;
using QSLeaderboard.UI;
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
            List<ImageHolder> Imageholder = Enumerable.Range(0, 10).Select(x => new ImageHolder(x)).ToList();
            Container.Bind<List<ImageHolder>>().FromInstance(Imageholder).AsSingle().WhenInjectedInto<LeaderboardView>();

            ScoreInfoModal scoreInfoModal = new ScoreInfoModal();
            List<ButtonHolder> buttonholder = Enumerable.Range(0, 10).Select(x => new ButtonHolder(x, scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.Bind<List<ButtonHolder>>().FromInstance(buttonholder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
