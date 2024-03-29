﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BloqParty.Utils;
using HMUI;
using IPA.Loader;
using IPA.Utilities;
using IPA.Utilities.Async;
using LeaderboardCore.Interfaces;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using static BloqParty.Utils.UIUtils;
using Button = UnityEngine.UI.Button;

namespace BloqParty.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/LeaderboardView.bsml")]
    [ViewDefinition("BloqParty.UI.Leaderboard.BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet, IInitializable
    {
        [Inject] private readonly PlatformLeaderboardViewController _plvc;
        [Inject] private readonly PlayerUtils _playerUtils;
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly RequestUtils _requestUtils;
        [Inject] private readonly LeaderboardData _leaderboardData;
        [Inject] private readonly ResultsViewController _resultsViewController;
        [Inject] private readonly UIUtils _uiUtils;
        [Inject] private readonly TweeningService _tweeningService;
        [Inject] private readonly AuthenticationManager _authenticationManager;
        [Inject] private readonly SiraLog _log;

        internal IDifficultyBeatmap currentDifficultyBeatmap;
        internal IDifficultyBeatmapSet currentDifficultyBeatmapSet;
        private CancellationTokenSource cancellationTokenSource;

        private List<LeaderboardData.LeaderboardEntry> currentEntries;
        private string currentSongLinkLBWebView = string.Empty;
        internal static LeaderboardData.LeaderboardEntry[] buttonEntryArray = new LeaderboardData.LeaderboardEntry[10];
        internal string sortMethod = "top";
        internal int season = 0;

        [UIComponent("leaderboardTableView")]
        private readonly LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        internal readonly Transform leaderboardTransform = null;

        [UIComponent("myHeader")]
        private readonly Backgroundable myHeader;

        [UIComponent("headerText")]
        private readonly TextMeshProUGUI headerText;

        [UIComponent("errorText")]
        private readonly TextMeshProUGUI errorText;

        [UIValue("imageHolders")]
        [Inject] internal List<ImageHolder> _ImageHolders;

        [UIValue("buttonHolders")]
        [Inject] internal List<EntryHolder> Buttonholders;

        [UIComponent("scoreInfoModal")]
        [Inject] internal readonly ScoreInfoModal scoreInfoModal;

        [UIComponent("up_button")]
        private readonly Button up_button;

        [UIComponent("down_button")]
        private readonly Button down_button;

        [UIObject("loadingLB")]
        private readonly GameObject loadingLB;

        [UIValue("versionText")]
        private readonly string versionText = $"You are running BloqParty v{PluginManager.GetPlugin("BloqParty").HVersion} on Beat Saber v{UnityGame.GameVersion.ToString().Split('_')[0]}";

        [UIAction("DownloadPlaylistSelect")]
        private void DownloadPlaylistSelect() => Application.OpenURL($"{Constants.BASE_WEB_URL}/{season}");

        [UIAction("OpenLeaderboardSelect")]
        private void OpenLeaderboardSelect()
        {
            Application.OpenURL($"{Constants.BASE_WEB_URL}/leaderboard/{season}");
        }

        private int currentSeason;

        [UIComponent("seasonList")]
        public CustomCellListTableData seasonList;

        [UIValue("seasonsContents")]
        private List<object> seasonsContents => new List<object>();

        internal void SetSeasonList(int _currentSeason)
        {
            currentSeason = _currentSeason;
            List<SeasonListItem> seasonButtons = Enumerable.Range(0, _currentSeason)
                .Select(i =>
                {
                    if (_currentSeason - i == _currentSeason)
                    {
                        return new SeasonListItem(_currentSeason, $"Season {_currentSeason}", "Speed Tech", Utilities.FindSpriteInAssembly("BloqParty.Images.logo.png"), "Rank: 1", "PP: 10234");
                    }
                    return new SeasonListItem(_currentSeason - i, $"Season {_currentSeason - i}", "No Pauses", Utilities.FindSpriteInAssembly("BloqParty.Images.logo.png"), "Rank: 53", "PP: 123");
                }).ToList();
            seasonList.data = seasonButtons.Cast<object>().ToList();
            seasonList.tableView.ReloadData();
        }

        internal int page = 0;
        internal int totalPages;

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

        internal void UpdatePageButtons()
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

        [UIAction("OpenLeaderboardPage")]
        internal void OpenLeaderboardPage()
        {
            if (!(string.IsNullOrEmpty(currentSongLinkLBWebView) || currentSongLinkLBWebView.Contains(" ")))
            {
                Application.OpenURL(currentSongLinkLBWebView);
            }
        }

        [UIAction("OpenBugPage")]
        internal void OpenBugPage() => Application.OpenURL(Constants.BUG_REPORT_LINK);

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
            new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("BloqParty.Images.globe.png"), "Bloq Party"),
            new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("BloqParty.Images.player.png"), "Around you")
        };

        internal void SetErrorState(bool active, string reason)
        {
            errorText.gameObject.SetActive(active);
            errorText.text = reason;
        }

        internal void showInfoModal()
        {
            parserParams.EmitEvent("hideSeasonSelectModal");
            parserParams.EmitEvent("hideScoreInfo");
            parserParams.EmitEvent("showInfoModal");
        }

        [UIAction("hideDaModalBruh")]
        internal void hideDaModalBruh()
        {
            parserParams.EmitEvent("hideSeasonSelectModal");
        }

        internal void showSeasonSelectModal()
        {
            parserParams.EmitEvent("showSeasonSelectModal");
            parserParams.EmitEvent("hideScoreInfo");
            parserParams.EmitEvent("hideInfoModal");
        }

        [UIAction("openWebsite")]
        internal void openWebsite()
        {
            Application.OpenURL("https://thebedroom.party");
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!base.isActiveAndEnabled) return;
            if (!_plvc) return;

            Transform header = _plvc.transform.Find("HeaderPanel");
            if (firstActivation)
            {
                _panelView.seasonText.richText = true;
                UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    Task.Run(() => _requestUtils.HandleLBAuth());
                    _uiUtils.GetCoolMaterialAndApply();
                });
                TextHoverEffect textHoverEffect = _panelView.playerUsername.gameObject.AddComponent<UIUtils.TextHoverEffect>();
                textHoverEffect.daComponent = _panelView.playerUsername;
                textHoverEffect.daStyle = FontStyles.Underline;
                textHoverEffect.origStyle = FontStyles.Normal;
            }
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc || !_plvc.isActivated) return;
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            if (!_plvc.isActivated) return;
            page = 0;
            parserParams.EmitEvent("hideInfoModal");
            parserParams.EmitEvent("hideSeasonSelectModal");
            parserParams.EmitEvent("hideScoreInfo");
        }

        void FadeOut(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                if (!(cell.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)) continue;
                _tweeningService.FadeText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText"), false, 0.3f);
                _tweeningService.FadeText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText"), false, 0.3f);
                _tweeningService.FadeText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText"), false, 0.3f);
            }
        }

        private void HandleNoLeaderboardEntries()
        {
            if (!errorText.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                _tweeningService.FadeText(errorText, true, 0.3f);
            }
            if (leaderboardTableView.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                FadeOut(leaderboardTableView);
            }
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            UnityMainThreadTaskScheduler.Factory.StartNew(() => realLeaderboardSet(difficultyBeatmap, cancellationToken));
        }


        private async Task realLeaderboardSet(IDifficultyBeatmap difficultyBeatmap, CancellationToken cancellationToken)
        {
            if (!_plvc || !_plvc.isActiveAndEnabled) return;

            leaderboardTableView.SetScores(null, -1);
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            cancellationToken.ThrowIfCancellationRequested();

            if (!_authenticationManager.IsAuthed) return;
            try
            {
                if (!_plvc) return;
                _log.Info("Setting leaderboard");
                scoreInfoModal.hidethefucker();
                SetErrorState(false, "");
                leaderboardTableView.SetScores(null, -1);
                loadingLB.gameObject.SetActive(true);
                _uiUtils.ByeImages();

                SetErrorState(false, "");
                leaderboardTableView.SetScores(null, -1);
                loadingLB.gameObject.SetActive(true);
                _uiUtils.ByeImages();

                string mapId = difficultyBeatmap.level.levelID.Substring(13);
                mapId = mapId.Split('_')[0];
                int difficulty = difficultyBeatmap.difficultyRank;
                string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
                string balls = mapId + "_" + mapType + difficulty.ToString(); // BeatMap Allocated Level Label String
                currentSongLinkLBWebView = $"https://thebedroom.party/leaderboard/{mapId}";

                await Task.Delay(200);

                if (cancellationToken.IsCancellationRequested)
                {
                    SetErrorState(false, "");
                    loadingLB.gameObject.SetActive(true);
                    _uiUtils.ByeIMGLoader();
                    leaderboardTableView.SetScores(null, -1);
                    FadeOut(leaderboardTableView);
                    return;
                }

                _log.Info("Getting leaderboard data");
                _requestUtils.GetBeatMapData((mapId, difficulty, mapType), page, result =>
                {
                    currentEntries = result.Item2;
                    totalPages = result.Item3;
                    _uiUtils.HelloIMGLoader();
                    UpdatePageButtons();
                    if (result.Item2 != null)
                    {
                        if (result.Item2.Count == 0)
                        {
                            _log.Info("No Scores Found");
                            SetErrorState(false, "No Scores Found");
                            HandleNoLeaderboardEntries();
                            loadingLB.gameObject.SetActive(false);
                            _uiUtils.ByeIMGLoader();
                        }
                        else
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                SetErrorState(false, "");
                                loadingLB.gameObject.SetActive(true);
                                _uiUtils.ByeIMGLoader();
                                leaderboardTableView.SetScores(null, -1);
                                return;
                            }
                            loadingLB.gameObject.SetActive(false);
                            leaderboardTableView.SetScores(LeaderboardDataUtils.CreateLeaderboardData(result.Item2, page, Buttonholders), LeaderboardDataUtils.GetUserScorePos(result.Item2, _authenticationManager._localPlayerInfo.userID));
                            currentEntries = result.Item2;
                            _uiUtils.RichMyText(leaderboardTableView);
                            _uiUtils.SetProfiles(result.Item2, cancellationToken);
                            _log.Info("Finished setting leaderboard");
                        }
                    }
                    else
                    {
                        _log.Error("Error getting leaderboard");
                        SetErrorState(false, "Error");
                        HandleNoLeaderboardEntries();
                        loadingLB.gameObject.SetActive(false);
                        _uiUtils.ByeIMGLoader();
                    }
                });
                if (cancellationToken.IsCancellationRequested)
                {
                    SetErrorState(false, "");
                    loadingLB.gameObject.SetActive(true);
                    _uiUtils.ByeIMGLoader();
                    leaderboardTableView.SetScores(null, -1);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                _log.Info("Operation Cancelled");
                SetErrorState(false, "");
                loadingLB.gameObject.SetActive(true);
                _uiUtils.ByeIMGLoader();
                leaderboardTableView.SetScores(null, -1);
                return;
            }
        }

        internal bool hasClickedOffResultsScreen = false;

        public void Initialize()
        {
            _resultsViewController.continueButtonPressedEvent += FUCKOFFIHATETHISIWANTTODIE;
        }

        internal void FUCKOFFIHATETHISIWANTTODIE(ResultsViewController resultsViewController)
        {
            hasClickedOffResultsScreen = true;
        }
    }
}
