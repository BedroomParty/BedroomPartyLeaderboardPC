using BloqParty.AffinityPatches;
using BloqParty.UI;
using BloqParty.UI.Leaderboard;
using BloqParty.Utils;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using static BloqParty.Utils.UIUtils;

namespace BloqParty.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<HeadsetUtils>().AsSingle();
            Container.BindInterfacesTo<LeaderboardUtils>().AsSingle();
            Container.Bind<PlayerUtils>().AsSingle();
            Container.Bind<AuthenticationManager>().AsSingle();
            Container.Bind<RequestUtils>().AsSingle();
            Container.Bind<UIUtils>().AsSingle();
            Container.BindInterfacesTo<Results>().AsSingle();
            Container.BindInterfacesTo<AuthPatch>().AsSingle();
            Container.Bind<TweeningService>().AsSingle();
            Container.Bind<LeaderboardData>().AsSingle();
            List<ImageHolder> Imageholder = Enumerable.Range(0, 10).Select(x => new ImageHolder(x)).ToList();
            Container.Bind<List<ImageHolder>>().FromInstance(Imageholder).AsSingle().WhenInjectedInto<LeaderboardView>();
            ScoreInfoModal scoreInfoModal = new();
            List<EntryHolder> entryHolder = Enumerable.Range(0, 10).Select(x => new EntryHolder(x, scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.Bind<List<EntryHolder>>().FromInstance(entryHolder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
