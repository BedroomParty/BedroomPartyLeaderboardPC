using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BedroomPartyLeaderboard.Utils;
using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using LeaderboardCore.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using static BedroomPartyLeaderboard.Utils.UIUtils;
using static LeaderboardTableView;
using Button = UnityEngine.UI.Button;

namespace BedroomPartyLeaderboard.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/LeaderboardView.bsml")]
    [ViewDefinition("BedroomPartyLeaderboard.UI.Leaderboard.BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet, IInitializable
    {
        [Inject] private readonly PlatformLeaderboardViewController _plvc;
        [Inject] private readonly PlayerUtils _playerUtils;
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly RequestUtils _requestUtils;
        [Inject] private readonly LeaderboardData _leaderboardData;
        [Inject] private readonly ResultsViewController _resultsViewController;
        [Inject] private readonly UIUtils _uiUtils;

        public IDifficultyBeatmap currentDifficultyBeatmap;
        public IDifficultyBeatmapSet currentDifficultyBeatmapSet;

        private string currentSongLinkLBWebView = string.Empty;
        public static LeaderboardData.LeaderboardEntry[] buttonEntryArray = new LeaderboardData.LeaderboardEntry[10];
        public string sortMethod = "top";

        [UIComponent("leaderboardTableView")]
        private readonly LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        private readonly Transform leaderboardTransform = null;

        [UIComponent("myHeader")]
        private readonly Backgroundable myHeader;

        [UIComponent("headerText")]
        private readonly TextMeshProUGUI headerText;

        [UIComponent("errorText")]
        private readonly TextMeshProUGUI errorText;

        [UIValue("imageHolders")]
        [Inject] public List<ImageHolder> _ImageHolders;

        [UIValue("buttonHolders")]
        [Inject] private readonly List<ButtonHolder> Buttonholders;

        [UIComponent("scoreInfoModal")]
        [Inject] private readonly ScoreInfoModal scoreInfoModal;

        [UIComponent("up_button")]
        private readonly Button up_button;

        [UIComponent("down_button")]
        private readonly Button down_button;

        [UIObject("loadingLB")]
        private readonly GameObject loadingLB;

        public int page = 0;
        public int totalPages;

        [UIAction("OnPageUp")]
        private void OnPageUp()
        {
            UpdatePageChanged(-1);
        }

        [UIAction("OnPageDown")]
        private void OnPageDown()
        {
            UpdatePageChanged(1);
        }

        private void UpdatePageChanged(int inc)
        {
            page = Mathf.Clamp(page + inc, 0, totalPages - 1);
            UpdatePageButtons();
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        public void UpdatePageButtons()
        {
            if (sortMethod == "around")
            {
                up_button.interactable = false;
                down_button.interactable = false;
                return;
            }
            up_button.interactable = page > 1;
            down_button.interactable = page < totalPages - 1;
        }

        [UIParams]
        private readonly BSMLParserParams parserParams;

        private GameObject _loadingControl;
        private ImageView _imgView;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [UIAction("#post-parse")]
        private void PostParse()
        {
            myHeader.background.material = Utilities.ImageResources.NoGlowMat;
            _loadingControl = leaderboardTransform.Find("LoadingControl").gameObject;
            Transform loadingContainer = _loadingControl.transform.Find("LoadingContainer");
            loadingContainer.gameObject.SetActive(false);
            Destroy(loadingContainer.Find("Text").gameObject);
            Destroy(_loadingControl.transform.Find("RefreshContainer").gameObject);
            Destroy(_loadingControl.transform.Find("DownloadingContainer").gameObject);
            _imgView = myHeader.background as ImageView;
            _imgView.color = Constants.BP_COLOR;
            _imgView.color0 = Constants.BP_COLOR;
            _imgView.color1 = Constants.BP_COLOR;
            ImageSkew(ref _imgView) = 0.18f;
            ImageGradient(ref _imgView) = true;
        }
        private void FuckOffButtons()
        {
            Buttonholders.ForEach(Buttonholders => Buttonholders.infoButton.gameObject.SetActive(false));
        }

        [UIAction("openLBWebView")]
        public void openLBWebView()
        {
            if (!(string.IsNullOrEmpty(currentSongLinkLBWebView) || currentSongLinkLBWebView.Contains(" ")))
            {
                Application.OpenURL(currentSongLinkLBWebView);
            }
        }

        [UIAction("openBUGWebView")]
        public void openBUGWebView()
        {
            Application.OpenURL(Constants.BUG_REPORT_LINK);
        }

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index)
        {
            sortMethod = index == 0 ? "top" : index == 1 ? "around" : "top";

            page = 0;
            UpdatePageButtons();
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons => new()
                {
                    new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("BedroomPartyLeaderboard.Images.Globe.png"), "Bedroom Party"),
                    new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("BedroomPartyLeaderboard.Images.Player.png"), "Around you")
                };

        public void SetErrorState(bool active, string reason)
        {
            errorText.gameObject.SetActive(active);
            errorText.text = reason;
        }

        public void showInfoModal()
        {
            parserParams.EmitEvent("showInfoModal");
        }

        [UIAction("openWebsite")]
        public void openWebsite()
        {
            Application.OpenURL("https://thebedroom.party");
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!base.isActiveAndEnabled)
            {
                return;
            }

            if (!_plvc)
            {
                return;
            }

            Transform header = _plvc.transform.Find("HeaderPanel");
            if (firstActivation)
            {
                _panelView.prompt_loader.SetActive(true);
                _panelView.promptText.gameObject.SetActive(true);
                _panelView.promptText.text = "Authenticating...";
                _ = UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    _playerUtils.LoginUser();

                });
            }
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc)
            {
                return;
            }

            if (!_plvc.isActivated)
            {
                return;
            }

            page = 0;
            parserParams.EmitEvent("hideInfoModal");
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
            _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => realLeaderboardSet(difficultyBeatmap));
        }

        private async Task realLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            if (!_plvc || !_plvc.isActiveAndEnabled)
            {
                return;
            }

            await Task.Delay(1);
            leaderboardTableView.SetScores(null, -1);
            loadingLB.gameObject.SetActive(true);
            FuckOffButtons();
            ByeImages();

            if (!_playerUtils.IsAuthed)
            {
                SetErrorState(true, "Failed to Auth");
                return;
            }

            SetErrorState(false, "");


            if (!_plvc || !_plvc.isActiveAndEnabled)
            {
                return;
            }

            await Task.Delay(500);

            string mapId = difficultyBeatmap.level.levelID.Substring(13);
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapId + "_" + mapType + difficulty.ToString(); // BeatMap Allocated Level Label String
            currentSongLinkLBWebView = $"https://thebedroom.party/?board={balls}";
            _requestUtils.GetBeatMapData((mapId, difficulty, mapType), page, result =>
            {
                totalPages = result.Item3;
                HelloIMGLoader();
                UpdatePageButtons();
                if (result.Item2 != null)
                {
                    if (result.Item2.Count == 0)
                    {
                        SetErrorState(true, "No Scores Found");
                        loadingLB.gameObject.SetActive(false);
                        ByeIMGLoader();
                    }
                    else
                    {
                        loadingLB.gameObject.SetActive(false);
                        leaderboardTableView.SetScores(CreateLeaderboardData(result.Item2, page), -1);
                        _uiUtils.RichMyText(leaderboardTableView);
                        _uiUtils.SetProfiles(result.Item2);
                    }
                }
                else
                {
                    SetErrorState(true, "Error");
                    loadingLB.gameObject.SetActive(false);
                    ByeIMGLoader();
                    Plugin.Log.Error("Error");
                }
            });
        }

        private void ByeImages()
        {
            _ImageHolders.ForEach(holder => holder.profileImage.sprite= null);
        }

        private void HelloIMGLoader()
        {
            _ImageHolders.ForEach(holder => holder.profileloading.SetActive(true));
        }

        private void ByeIMGLoader()
        {
            _ImageHolders.ForEach(holder => holder.profileloading.SetActive(false));
        }

        public List<ScoreData> CreateLeaderboardData(List<LeaderboardData.LeaderboardEntry> leaderboard, int page)
        {
            List<ScoreData> tableData = new();
            for (int i = 0; i < leaderboard.Count; i++)
            {
                int score = leaderboard[i].modifiedScore;
                int rank = int.Parse(page.ToString()[0] + (i + 1).ToString());
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], score, rank));
                buttonEntryArray[i] = leaderboard[i];
                Buttonholders[i].infoButton.gameObject.SetActive(true);
            }
            return tableData;
        }

        public ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardEntry entry, int score, int rankFUCK)
        {
            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            string formattedCombo = entry.fullCombo
                ? " -<color=green> FC </color>"
                : string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);
            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;
            if (entry.userID == "76561199077754911")
            {
                entry.userName = $"<color=blue>{entry.userID}</color>";
            }

            result = "<size=90%>" + entry.userName.TrimEnd() + formattedAcc + formattedCombo + formattedMods + "</size>";
            entry.rank = rankFUCK;
            return new ScoreData(score, result, rankFUCK, false);
        }

        public void Initialize()
        {
            _resultsViewController.continueButtonPressedEvent += FUCKOFFIHATETHISIWANTTODIE;
        }

        public void FUCKOFFIHATETHISIWANTTODIE(ResultsViewController resultsViewController)
        {
            OnLeaderboardSet(currentDifficultyBeatmap);
        }
    }
}
