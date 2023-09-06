using BedroomPartyLeaderboard.AffinityPatches;
using BedroomPartyLeaderboard.UI;
using BedroomPartyLeaderboard.UI.Leaderboard;
using BedroomPartyLeaderboard.Utils;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using static BedroomPartyLeaderboard.Utils.UIUtils;

namespace BedroomPartyLeaderboard.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            _ = Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            _ = Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            _ = Container.BindInterfacesTo<LeaderboardUtils>().AsSingle();
            _ = Container.Bind<PlayerUtils>().AsSingle();
            _ = Container.Bind<RequestUtils>().AsSingle();
            _ = Container.Bind<UIUtils>().AsSingle();
            _ = Container.BindInterfacesTo<Results>().AsSingle();
            _ = Container.Bind<LeaderboardData>().AsSingle();
            List<ImageHolder> Imageholder = Enumerable.Range(0, 10).Select(x => new ImageHolder(x)).ToList();
            _ = Container.Bind<List<ImageHolder>>().FromInstance(Imageholder).AsSingle().WhenInjectedInto<LeaderboardView>();

            ScoreInfoModal scoreInfoModal = new();
            List<ButtonHolder> buttonholder = Enumerable.Range(0, 10).Select(x => new ButtonHolder(x, scoreInfoModal.setScoreModalText)).ToList();
            _ = Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            _ = Container.Bind<List<ButtonHolder>>().FromInstance(buttonholder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
