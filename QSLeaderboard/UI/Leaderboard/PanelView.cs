using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using QSLeaderboard.Utils;
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

        private const float _skew = 0.18f;
        private ImageView _background;
        public ImageView _imgView;

        [UIComponent("QSLeaderboard_logo")]
        private ImageView QSLeaderboard_logo;


        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");


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

        [UIAction("#post-parse")]
        private void PostParse()
        {
            
        }
    }
}
