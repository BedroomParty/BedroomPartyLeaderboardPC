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
using IPA.Utilities.Async;
using LeaderboardCore.Interfaces;
using QSLeaderboard.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static LeaderboardTableView;
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

        [UIObject("loadingLB")]
        private GameObject loadingLB;

        [UIAction("OnPageUp")]
        public void OnPageUp(bool tooFar)
        {
            page--;
            if (tooFar) totalPages = page;
            UpdatePageChanged();
            UpdatePageButtons();
        }

        [UIAction("OnPageDown")]
        public void OnPageDown()
        {
            page++;
            UpdatePageChanged();
            UpdatePageButtons();
        }

        [UIParams]
        BSMLParserParams parserParams;

        public void UpdatePageButtons()
        {
            up_button.interactable = (page > 0);
            down_button.interactable = (page < totalPages);
        }

        public void UpdatePageChanged()
        {
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
            if (!base.isActiveAndEnabled) return;
            if (!_plvc) return;

            if (currentDifficultyBeatmap != null) OnLeaderboardSet(currentDifficultyBeatmap);
            if (firstActivation)
            {
                _playerUtils.GetAuthStatus(result =>
                {
                    bool isAuthenticated = result.Item1;
                    string username = result.Item2;

                    if (isAuthenticated)
                    {
                        Plugin.Authed = true;
                        Plugin.userName = username;
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

                if(Plugin.Authed)
                {
                    OnLeaderboardSet(currentDifficultyBeatmap);
                }
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
            if (!_plvc || !_plvc.isActiveAndEnabled) return;

            if (!Plugin.Authed) return;

            currentDifficultyBeatmap = difficultyBeatmap;
            
            loadingLB.gameObject.SetActive(true);
            leaderboardTableView.SetScores(null, -1);
            errorText.gameObject.SetActive(false);


            string mapId = difficultyBeatmap.level.levelID;
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapId + mapType + difficulty.ToString(); // BeatMap Allocated Level Label String


            _requestUtils.GetBeatMapData(balls, page, result =>
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    if (!_plvc || !_plvc.isActiveAndEnabled) return;
                    if (result.Item2 != null)
                    {
                        if (result.Item2.Count == 0)
                        {
                            leaderboardTableView.SetScores(null, -1);
                            errorText.gameObject.SetActive(true);
                        }
                        else
                        {
                            leaderboardTableView.SetScores(CreateLeaderboardData(result.Item2, page), -1);
                            RichMyText(leaderboardTableView);
                            loadingLB.gameObject.SetActive(false);   
                        }
                    }
                    else
                    {
                        loadingLB.gameObject.SetActive(false);
                        leaderboardTableView.SetScores(null, -1);
                        errorText.gameObject.SetActive(true);
                    }
                });
            });
        }

        void RichMyText(LeaderboardTableView tableView) 
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                cell.showSeparator = true;
                var nameText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");
                var rankText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText");
                var scoreText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText");
                nameText.richText = true;
            }
        }

        public List<ScoreData> CreateLeaderboardData(List<LeaderboardData.LeaderboardEntry> leaderboard, int page)
        {
            Plugin.Log.Notice("Creating LB DATA");
            List<ScoreData> tableData = new List<ScoreData>();
            /*
            int pageIndex = page * 10;
            for (int i = pageIndex; i < leaderboard.Count && i < pageIndex + 10; i++)
            {

                Plugin.Log.Notice($"Creating LB DATA at - {i}");
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + 1, score));
            }
            */

            for (int i = 0; i < leaderboard.Count; i++)
            {
                Plugin.Log.Notice($"Creating LB DATA at - {i}");
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + (page * 10) + 1, score));
            }

            return tableData;
        }

        public ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardEntry entry, int rank, int score)
        {

            Plugin.Log.Notice("Creating LB ENTRY");

            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);

            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;

            result = "<size=100%>" + entry.userName + formattedAcc + formattedCombo + formattedMods + "</size>";

            return new ScoreData(score, result, rank, false);
        }
    }
}
