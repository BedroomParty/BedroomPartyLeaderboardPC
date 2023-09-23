using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BedroomPartyLeaderboard.Utils;
using HMUI;
using TMPro;
using UnityEngine;
using Zenject;

namespace BedroomPartyLeaderboard.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/PanelView.bsml")]
    [ViewDefinition("BedroomPartyLeaderboard.UI.Leaderboard.BSML.PanelView.bsml")]
    internal class PanelView : BSMLAutomaticViewController
    {
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly AuthenticationManager _authenticationManager;

        [UIComponent("BedroomPartyLeaderboard_logo")]
        private readonly ImageView BedroomPartyLeaderboard_logo;

        [UIObject("prompt_loader")]
        public GameObject prompt_loader;

        [UIComponent("promptText")]
        public TextMeshProUGUI promptText;

        [UIComponent("playerAvatar")]
        public HMUI.ImageView playerAvatar;

        [UIObject("playerAvatarLoading")]
        public GameObject playerAvatarLoading;

        [UIComponent("playerUsername")]
        public TextMeshProUGUI playerUsername;

        [UIComponent("seasonText")]
        public TextMeshProUGUI seasonText;

        [UIComponent("pointsText")]
        public TextMeshProUGUI ppText;

        [UIComponent("notiImage")]
        public ImageView notiImage;

        [UIAction("FunnyModalMoment")]
        public void FunnyModalMoment()
        {
            _leaderboardView.showInfoModal();
        }

        [UIAction("playerUsernameCLICK")]
        public void playerUsernameCLICK()
        {
            if (string.IsNullOrEmpty(_authenticationManager._localPlayerInfo.userID))
            {
                return;
            }

            Application.OpenURL($"https://thebedroom.party/?user={_authenticationManager._localPlayerInfo.userID}");
        }

        [UIAction("seasonTextCLICK")]
        public void seasonTextCLICK()
        {
            _leaderboardView.showSeasonSelectModal();
        }
    }
}
