using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using TMPro;
using UnityEngine;
using Zenject;

namespace QSLeaderboard.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/PanelView.bsml")]
    [ViewDefinition("QSLeaderboard.UI.Leaderboard.BSML.PanelView.bsml")]
    internal class PanelView : BSMLAutomaticViewController
    {
        [Inject] LeaderboardView _leaderboardView;

        [UIComponent("QSLeaderboard_logo")]
        private ImageView QSLeaderboard_logo;

        [UIObject("prompt_loader")]
        public GameObject prompt_loader;

        [UIComponent("promptText")]
        public TextMeshProUGUI promptText;

        [UIComponent("playerAvatar")]
        public HMUI.ImageView playerAvatar;

        [UIObject("playerAvatarLoading")]
        public GameObject playerAvatarLoading;

        [UIComponent("playerGlobalRank")]
        public TextMeshProUGUI playerGlobalRank;

        [UIComponent("playerPP")]
        public TextMeshProUGUI playerPP;

        [UIComponent("playerUsername")]
        public TextMeshProUGUI playerUsername;

        [UIAction("FunnyModalMoment")]
        public void FunnyModalMoment()
        {
            _leaderboardView.showInfoModal();
        }

        [UIAction("playerUsernameCLICK")]
        public void playerUsernameCLICK()
        {
            if (string.IsNullOrEmpty(Plugin.discordID)) return;
            Application.OpenURL($"https://questsupporters.me/?user={Plugin.discordID}");
        }
    }
}
