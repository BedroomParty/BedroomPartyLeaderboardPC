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
using UnityEngine.Networking;
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

        public IDifficultyBeatmap currentDifficultyBeatmap;
        public IDifficultyBeatmapSet currentDifficultyBeatmapSet;


        public static ImageView[] profileImageArray = new ImageView[10];

        private Sprite transparentSprite;

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

        [UIValue("imageHolders")]
        [Inject] private List<ImageHolder> holders;

        [UIComponent("up_button")]
        private Button up_button;

        [UIComponent("down_button")]
        private Button down_button;

        [UIObject("loadingLB")]
        private GameObject loadingLB;

        public int page = 0;
        public int totalPages;
        public int sortMethod;

        [UIAction("OnPageUp")]
        private void OnPageUp() => UpdatePageChanged(-1);

        [UIAction("OnPageDown")]
        private void OnPageDown() => UpdatePageChanged(1);

        private void UpdatePageChanged(int inc)
        {
            page = Mathf.Clamp(page + inc, 0, totalPages - 1);
            UpdatePageButtons();
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        private void UpdatePageButtons()
        {
            up_button.interactable = (page > 0);
            down_button.interactable = (page < totalPages - 1);
        }

        [UIParams]
        BSMLParserParams parserParams;

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

        private async void SetProfileImage(string url, int index)
        {
            ImageView image = holders[index].profileImage;
            GameObject loader = holders[index].profileloading;

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Delay(10);
            }

            if (request.responseCode == 200)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                // Use Unity's main thread scheduler factory to update the image on the main thread
                await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    image.sprite = sprite;
                    loader.SetActive(false);
                });
            }
            else
            {
                Debug.LogError("Failed to retrieve profile image: " + request.error);
            }

            request.Dispose();
        }


        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index)
        {
            sortMethod = index;
            page = 0;
            UpdatePageButtons();
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
                        if (currentDifficultyBeatmap != null)
                        {
                            OnLeaderboardSet(currentDifficultyBeatmap);
                            UpdatePageButtons();
                        }
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
                // Create a transparent texture with a size of 1x1
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f)); // Set pixel color with alpha = 0 (transparent)
                texture.Apply();

                // Create a sprite using the transparent texture
                transparentSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
            }
        }



        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            page = 0;
            if (!_plvc) return;
            if (!_plvc.isActivated) return;
            parserParams.EmitEvent("hideInfoModal");
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            bool shouldUpdatePage;
            if(currentDifficultyBeatmap != difficultyBeatmap)
            {
                shouldUpdatePage = true;
            }
            else
            {
                shouldUpdatePage = false;
            }
            currentDifficultyBeatmap = difficultyBeatmap;
            if (!_plvc || !_plvc.isActiveAndEnabled) return;

            if (!Plugin.Authed)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Unable to authenticate, please restart";
                return;
            }

            loadingLB.gameObject.SetActive(true);
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
                    FuckOffImages();
                    HelloLoadImages();
                    if (result.Item2 != null)
                    {
                        if (result.Item2.Count == 0)
                        {
                            leaderboardTableView.SetScores(null, -1);
                            errorText.gameObject.SetActive(true);
                            FuckLoadImages();
                            UpdatePageButtons();
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
                        _panelView.isMapRanked.text = "Ranked Status: Unranked";
                        FuckLoadImages();
                    }
                    SetProfiles(result.Item2);
                    _panelView.currentRank.text = $"Current Rank: {result.Item3}";
                    totalPages = result.Item4;
                    Plugin.Log.Info("total Pages: " + totalPages.ToString());
                    Plugin.Log.Info($"stars: {result.Item5}");
                    bool isUnRanked = result.Item5 == 0;
                    Plugin.Log.Info($"Ranked: {isUnRanked.ToString()}");
                    _panelView.isMapRanked.text = isUnRanked ? "Ranked Status: Unranked" : "Ranked Status: Ranked";
                    UpdatePageButtons();
                });
            });
        }

        private void FuckOffImages() => holders.ForEach(holder => holder.profileImage.sprite = transparentSprite);
        private void HelloLoadImages() => holders.ForEach(holder => holder.profileloading.SetActive(true));
        private void FuckLoadImages() => holders.ForEach(holder => holder.profileloading.SetActive(false));



        void SetProfiles(List<LeaderboardData.LeaderboardEntry> leaderboard)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                var url = $"{Constants.PROFILE_PICTURE}/{leaderboard[i].userID.ToString()}/avatar";
                SetProfileImage(url, i);
            }

            for (int i = leaderboard.Count; i <= 10; i++)
            {
                holders[i].profileloading.gameObject.SetActive(false);
            }
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
            List<ScoreData> tableData = new List<ScoreData>();
            for (int i = 0; i < leaderboard.Count; i++)
            {
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + (page * 10) + 1, score));
                holders[i].profileImage.gameObject.SetActive(true);
            }
            return tableData;
        }

        public ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardEntry entry, int rank, int score)
        {
            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);

            string formattedPP = string.Empty;
            if(entry.PP > 0) formattedPP = $" - <color=blue>{entry.PP}</color>";
            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;

            result = "<size=100%>" + entry.userName + formattedAcc + formattedCombo + formattedPP + formattedMods + "</size>";

            return new ScoreData(score, result, rank, false);
        }
    }


    internal class ImageHolder
    {
        private int index;

        public ImageHolder(int index)
        {
            this.index = index;
        }

        [UIComponent("profileImage")]
        public ImageView profileImage;

        [UIObject("profileloading")]
        public GameObject profileloading;
    }
}
