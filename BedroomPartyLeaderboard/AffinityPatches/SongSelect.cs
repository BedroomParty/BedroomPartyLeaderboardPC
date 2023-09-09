using BedroomPartyLeaderboard.Utils;
using SiraUtil.Affinity;
using System.Threading.Tasks;
using Zenject;

namespace BedroomPartyLeaderboard.AffinityPatches
{

    internal class SongSelect : IAffinity
    {
        [Inject] private readonly PlayerUtils _playerUtils;

        [AffinityPostfix]
        [AffinityPatch(typeof(PlatformLeaderboardViewController), "DidActivate")]
        public void Postfix(ref bool firstActivation)
        {
            if (!firstActivation)
            {
                return;
            }
            Task.Run(() => _playerUtils.LoginUserAsync());
        }
    }
}
