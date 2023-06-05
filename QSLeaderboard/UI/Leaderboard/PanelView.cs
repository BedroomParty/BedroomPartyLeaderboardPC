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


        [UIComponent("container")]
        private Backgroundable _container;

        [UIComponent("QSLeaderboard_logo")]
        private ImageView QSLeaderboard_logo;

        [UIComponent("separator")]
        private ImageView _separator;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [UIComponent("currentRank")]
        public TextMeshProUGUI currentRank;

        [UIComponent("isMapRanked")]
        public TextMeshProUGUI isMapRanked;

        [UIComponent("promptText")]
        public TextMeshProUGUI promptText;

        [UIObject("prompt_loader")]
        public GameObject prompt_loader;

        [UIAction("FunnyModalMoment")]
        public void FunnyModalMoment()
        {
            _leaderboardView.showInfoModal();
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _container.background.material = Utilities.ImageResources.NoGlowMat;
            _imgView = _container.background as ImageView;
            _imgView.transform.position.Set(-5f, _imgView.transform.position.y, _imgView.transform.position.z);

            _imgView.color = Constants.QS_COLOR;
            _imgView.color0 = Color.white;
            _imgView.color1 = new Color(1, 1, 1, 0);

            ImageSkew(ref _imgView) = _skew;
            ImageGradient(ref _imgView) = true;

            ImageSkew(ref QSLeaderboard_logo) = _skew;
            QSLeaderboard_logo.SetVerticesDirty();
            ImageSkew(ref _separator) = _skew;
        }
    }
}
