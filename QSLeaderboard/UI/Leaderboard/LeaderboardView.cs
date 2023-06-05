using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using LeaderboardCore.Interfaces;
using QSLeaderboard.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = UnityEngine.UI.Button;

namespace QSLeaderboard.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/LeaderboardView.bsml")]
    [ViewDefinition("QSLeaderboard.UI.Leaderboard.BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet
    {
        [Inject] PlatformLeaderboardViewController _plvc;
        [Inject] PlayerUtils _playerUtils;
        [Inject] PanelView _panelView;
        [Inject] RequestUtils _requestUtils;
        [Inject] LeaderboardData _leaderboardData;

        public int page = 0;
        public int totalPages;
        public int sortMethod;

        private IDifficultyBeatmap currentDifficultyBeatmap;

        [UIComponent("leaderboardTableView")]
        private LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        private Transform leaderboardTransform = null;

        [UIComponent("myHeader")]
        private Backgroundable myHeader;

        [UIComponent("errorText")]
        private TextMeshProUGUI errorText;

        [UIComponent("userIDHere")]
        public TextMeshProUGUI userIDHere;

        [UIComponent("linkText")]
        public TextMeshProUGUI linkText;

        [UIComponent("up_button")]
        private Button up_button;

        [UIComponent("down_button")]
        private Button down_button;

        [UIAction("OnPageUp")]
        private void OnPageUp() => UpdatePageChanged(-1);

        [UIAction("OnPageDown")]
        private void OnPageDown() => UpdatePageChanged(1);

        [UIParams]
        BSMLParserParams parserParams;

        private void UpdatePageButtons()
        {
            up_button.interactable = (page > 0);
            down_button.interactable = (page < totalPages - 1);
        }

        private void UpdatePageChanged(int inc)
        {
            page = Mathf.Clamp(page + inc, 0, totalPages - 1);
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        private ImageView _imgView;
        private GameObject _loadingControl;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [UIAction("#post-parse")]
        private void PostParse()
        {
            myHeader.background.material = Utilities.ImageResources.NoGlowMat;
            _loadingControl = leaderboardTransform.Find("LoadingControl").gameObject;
            var loadingContainer = _loadingControl.transform.Find("LoadingContainer");
            loadingContainer.gameObject.SetActive(false);
            Destroy(loadingContainer.Find("Text").gameObject);
            Destroy(_loadingControl.transform.Find("RefreshContainer").gameObject);
            Destroy(_loadingControl.transform.Find("DownloadingContainer").gameObject);

            _imgView = myHeader.background as ImageView;
            _imgView.color = Constants.QS_COLOR;
            _imgView.color0 = Constants.QS_COLOR;
            _imgView.color1 = Constants.QS_COLOR;
            ImageSkew(ref _imgView) = 0.18f;
            ImageGradient(ref _imgView) = true;

            linkText.text = "Link your account with <size=110%><color=green>/link</color></size> in the QS server!";

        }

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index)
        {
            sortMethod = index;
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons
        {
            get
            {
                return new List<IconSegmentedControl.DataItem>()
                {
                new IconSegmentedControl.DataItem(
                    Utilities.FindSpriteInAssembly("QSLeaderboard.Images.Globe.png"), "Quest Supporters"),
                new IconSegmentedControl.DataItem(
                    Utilities.FindSpriteInAssembly("QSLeaderboard.Images.Player.png"), "Around you")
                };
            }
        }

        public void showInfoModal()
        {
            parserParams.EmitEvent("showInfoModal");
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!this.isActiveAndEnabled) return;
            if (!_plvc) return;
            OnLeaderboardSet(currentDifficultyBeatmap);
            if (firstActivation)
            {
                _playerUtils.GetAuthStatus(result =>
                {
                    bool isAuthenticated = result.Item1;
                    string username = result.Item2;

                    if (isAuthenticated)
                    {
                        Plugin.Log.Info("Authenticated! Username: " + username);
                        _panelView.prompt_loader.SetActive(false);
                        _panelView.promptText.text = $"<color=green>Successfully signed {username} in!</color>";
                    }
                    else
                    {
                        _panelView.promptText.text = "<color=red>Error Authenticating</color>";
                        _panelView.prompt_loader.SetActive(false);
                        Plugin.Log.Error("Not authenticated! Username: " + username);
                    }
                });


                _panelView.currentRank.text = $"Current Rank: #1";

                _panelView.isMapRanked.text = $"Ranked Status: Ranked";
            }
        }



        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc) return;
            if (!_plvc.isActivated) return;
            parserParams.EmitEvent("hideInfoModal");
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
            string mapId = difficultyBeatmap.level.levelID;
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapId + mapType + difficulty.ToString(); // BeatMap Allocated Level Label String

            Plugin.Log.Info("BEFORE REQUEST");
            _requestUtils.GetBeatMapData(balls, result =>
            {
                leaderboardTableView.SetScores(_leaderboardData.CreateLeaderboardData(result.Item2, 1), -1);
                Plugin.Log.Info("WITHIN RESULT");
                if (!result.Item1)
                {
                    Plugin.Log.Info("NO SCORES FOUND");
                    errorText.gameObject.SetActive(true);
                }
            });
            Plugin.Log.Info("AFTER REQUEST");
        }
    }
}
