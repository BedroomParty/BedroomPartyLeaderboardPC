using BedroomPartyLeaderboard.Utils;
using SiraUtil.Affinity;
using System.Threading.Tasks;
using Zenject;

namespace BedroomPartyLeaderboard.AffinityPatches
{

    internal class AuthPatch : IAffinity
    {
        [Inject] private readonly AuthenticationManager _authenticationManager;

        [AffinityPostfix]
        [AffinityPatch(typeof(PlatformLeaderboardViewController), "DidActivate")]
        public void Postfix(ref bool firstActivation)
        {
            if (!firstActivation)
            {
                return;
            }
            Task.Run(() => _authenticationManager.LoginUserAsync());
        }
    }
}
