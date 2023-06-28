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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;
using static LeaderboardTableView;
using Button = UnityEngine.UI.Button;

namespace QSLeaderboard.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/LeaderboardView.bsml")]
    [ViewDefinition("QSLeaderboard.UI.Leaderboard.BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet, IInitializable
    {
        [Inject] private PlatformLeaderboardViewController _plvc;
        [Inject] PlayerUtils _playerUtils;
        [Inject] PanelView _panelView;
        [Inject] RequestUtils _requestUtils;
        [Inject] LeaderboardData _leaderboardData;
        [Inject] private ResultsViewController _resultsViewController;

        public IDifficultyBeatmap currentDifficultyBeatmap;
        public IDifficultyBeatmapSet currentDifficultyBeatmapSet;


        public static ImageView[] profileImageArray = new ImageView[10];
        public Dictionary<string, Sprite> userSpriteDictionary = new Dictionary<string, Sprite>();
        private string currentSongLinkLBWebView = string.Empty;
        public static LeaderboardData.LeaderboardEntry[] buttonEntryArray = new LeaderboardData.LeaderboardEntry[10];

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



        [UIObject("loginKeyboard")]
        public GameObject loginKeyboard;

        [UIObject("loginButton")]
        public GameObject loginButton;


        [UIValue("imageHolders")]
        [Inject] private List<ImageHolder> holders;

        [UIValue("buttonHolders")]
        [Inject] private List<ButtonHolder> Buttonholders;

        [UIComponent("scoreInfoModal")]
        [Inject] private ScoreInfoModal scoreInfoModal;

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

        public async void SetProfileImage(string url, int index, string userID)
        {
            // Check if the sprite already exists in the dictionary
            if (userSpriteDictionary.ContainsKey(userID))
            {
                Plugin.Log.Info($"Sprite already exists for {userID}, using that!");
                holders[index].profileImage.sprite = userSpriteDictionary[userID];
                holders[index].profileloading.SetActive(false);
                return;
            }

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
                Texture2D roundedTexture = RoundTextureCorners(texture, 60f);
                Sprite sprite = Sprite.Create(roundedTexture, new Rect(0, 0, roundedTexture.width, roundedTexture.height), Vector2.one * 0.5f);

                if (!userSpriteDictionary.ContainsKey(userID))
                {
                    userSpriteDictionary.Add(userID, sprite);
                    Plugin.Log.Info($"Added {userID} to the cache!");
                }
                else
                {
                    Plugin.Log.Info($"Sprite already exists for {userID}, using that!");
                    sprite = userSpriteDictionary[userID];
                }

                await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    image.sprite = sprite;
                    loader.SetActive(false);
                });
            }
            else
            {
                Plugin.Log.Error("Failed to retrieve profile image: " + request.error);
            }

            request.Dispose();
        }
        private void FuckOffButtons() => Buttonholders.ForEach(Buttonholders => Buttonholders.infoButton.gameObject.SetActive(false));

        public Texture2D RoundTextureCorners(Texture2D texture, float cornerRadius)
        {
            int width = texture.width;
            int height = texture.height;
            Texture2D roundedTexture = new Texture2D(width, height);
            Color[] pixels = texture.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Color pixel = pixels[index];
                    Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
                    float distance = Vector2.Distance(new Vector2(x, y), center);

                    if (distance > cornerRadius)
                    {
                        pixel.a = 0f;
                    }

                    roundedTexture.SetPixel(x, y, pixel);
                }
            }

            roundedTexture.Apply();
            return roundedTexture;
        }

        [UIAction("openLBWebView")]
        public void openLBWebView()
        {
            if (String.IsNullOrEmpty(currentSongLinkLBWebView) || currentSongLinkLBWebView.Contains(" "))
            {
                return;
            }
            Application.OpenURL(currentSongLinkLBWebView);
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

        private protected string loginCODEFUCKOFF;

        [UIValue("loginKeyboardVALUE")]
        private string loginKeyboardVALUE
        {
            get => loginCODEFUCKOFF;
            set => loginCODEFUCKOFF = value;
        }

        [UIAction("loginButtonCLICK")]
        public void loginButtonCLICK()
        {
            if (int.TryParse(loginCODEFUCKOFF, out int silly))
            {
                LoginCode(silly);
            }
            else
            {

            }
        }


        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!base.isActiveAndEnabled) return;
            if (!_plvc) return;
            var header = _plvc.transform.Find("HeaderPanel");
            if (firstActivation)
            {
                int code = 0;
                string apiKey = string.Empty;
                if(File.Exists(Constants.BALL_PATH + "apiKey.txt"))
                {
                    apiKey = File.ReadAllText(Constants.BALL_PATH + "apiKey.txt");
                    LoginKey(apiKey);
                    Plugin.apiKey = apiKey;
                }
                else
                {
                    loginKeyboard.gameObject.SetActive(true);
                    loginButton.gameObject.SetActive(true);
                }

                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
                texture.Apply();

                transparentSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
            }
        }

        private protected void LoginCode(int code)
        {
            _playerUtils.GetAuthStatusCode(code, result =>
            {
                bool isAuthenticated = result.Item1;
                string username = result.Item2;

                if (isAuthenticated)
                {
                    Plugin.Authed = true;
                    Plugin.userName = username;
                    Plugin.Log.Info("Authenticated! Username: " + username);
                    loginButton.gameObject.SetActive(false);
                    loginKeyboard.gameObject.SetActive(false);
                    _panelView.prompt_loader.SetActive(false);
                    _panelView.promptText.text = $"<color=green>Successfully signed {username} in!</color>";
                    if (currentDifficultyBeatmap != null)
                    {
                        OnLeaderboardSet(currentDifficultyBeatmap);
                        UpdatePageButtons();
                    }
                    var url = $"{Constants.USER_URL}/{Plugin.userID}/avatar/low";
                    Plugin.Log.Info(url);
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => SetProfilePic(_panelView.playerAvatar, url));
                }
                else
                {
                    _panelView.promptText.text = "<color=red>Error Authenticating</color>";
                    _panelView.prompt_loader.SetActive(false);
                    Plugin.Log.Error("Not authenticated! Username: " + username);
                }
            });
        }

        private protected void LoginKey(string apiKey)
        {
            _playerUtils.GetAuthStatusKey(apiKey, result =>
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
                    var url = $"{Constants.USER_URL}/{Plugin.userID}/avatar/low";
                    Plugin.Log.Info(url);
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => SetProfilePic(_panelView.playerAvatar, url));
                }
                else
                {
                    _panelView.promptText.text = "<color=red>Error Authenticating</color>";
                    _panelView.prompt_loader.SetActive(false);
                    Plugin.Log.Error("Not authenticated! Username: " + username);
                }
            });
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc) return;
            if (!_plvc.isActivated) return;
            var header = _plvc.transform.Find("HeaderPanel");
            page = 0;
            parserParams.EmitEvent("hideInfoModal");
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
            UnityMainThreadTaskScheduler.Factory.StartNew(() => realLeaderboardSet(difficultyBeatmap));
        }

        private async Task SetProfilePic(ImageView image, string url)
        {
            Plugin.Log.Info(url);
            _panelView.playerAvatarLoading.SetActive(true);
            await Task.Delay(1);

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Delay(10);
            }

            if (request.responseCode == 200)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Texture2D roundedTexture = RoundTextureCorners(texture, 60f);
                Sprite sprite = Sprite.Create(roundedTexture, new Rect(0, 0, roundedTexture.width, roundedTexture.height), Vector2.one * 0.5f);

                await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    image.sprite = sprite;
                    _panelView.playerAvatarLoading.SetActive(false);
                });
            }
            else
            {
                Plugin.Log.Error("Failed to retrieve profile image: " + request.error);
            }

            request.Dispose();
        }



        private async Task realLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            string errorReason = "Error";
            if (!_plvc || !_plvc.isActiveAndEnabled) return;

            await Task.Delay(1);
            FuckOffButtons();

            if (!Plugin.Authed)
            {
                errorText.gameObject.SetActive(false);
                errorReason = "Auth Fail";
                return;
            }

            loadingLB.gameObject.SetActive(true);
            errorText.gameObject.SetActive(false);

            await Task.Delay(1);

            if (!_plvc || !_plvc.isActiveAndEnabled) return;

            string mapId = difficultyBeatmap.level.levelID.Substring(13);
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapId + "_" + mapType + difficulty.ToString(); // BeatMap Allocated Level Label String
            Plugin.Log.Info(balls);
            currentSongLinkLBWebView = $"https://questsupporters.me/?board={balls}";
            _requestUtils.GetBeatMapData(balls, page, result =>
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    if (!_plvc || !_plvc.isActiveAndEnabled) return;
                    FuckOffImages();
                    HelloLoadImages();

                    if (result.Item3 != 0)
                    {
                        _panelView.playerGlobalRank.text = $"#{result.Item3}";
                    }

                    _panelView.playerPP.text = $"{result.Item6}pp";

                    if (result.Item2 != null)
                    {
                        if (result.Item2.Count == 0)
                        {
                            errorReason = "No Scores Yet!";
                            leaderboardTableView.SetScores(null, -1);
                            errorText.gameObject.SetActive(true);
                            loadingLB.gameObject.SetActive(false);
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
                        errorReason = "No Scores Yet!";
                        leaderboardTableView.SetScores(null, -1);
                        errorText.gameObject.SetActive(true);
                        loadingLB.gameObject.SetActive(false);
                        FuckLoadImages();
                        errorText.gameObject.SetActive(true);
                    }
                    errorText.text = errorReason;
                    SetProfiles(result.Item2);
                    totalPages = result.Item4;
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
                var url = $"{Constants.USER_URL}/{leaderboard[i].userID.ToString()}/avatar/low";
                SetProfileImage(url, i, leaderboard[i].userID);
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
                rankText.richText = true;
                scoreText.richText = true;
                rankText.text = $"<size=120%><u>{rankText.text}</u></size>";
                var seperator = cell.GetField<Image, LeaderboardTableCell>("_separatorImage") as ImageView;
                seperator.color = Constants.QS_COLOR;
                seperator.color0 = Color.white;
                seperator.color1 = new Color(1, 1, 1, 0);
            }
        }

        public List<ScoreData> CreateLeaderboardData(List<LeaderboardData.LeaderboardEntry> leaderboard, int page)
        {
            List<ScoreData> tableData = new List<ScoreData>();
            for (int i = 0; i < leaderboard.Count; i++)
            {
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], score));
                holders[i].profileImage.gameObject.SetActive(true);
                buttonEntryArray[i] = leaderboard[i];
                Buttonholders[i].infoButton.gameObject.SetActive(true);
            }
            return tableData;
        }

        public ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardEntry entry, int score)
        {
            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);

            string formattedPP = string.Empty;
            if (entry.PP > 0) formattedPP = $" - <color=#AEC6CF>{entry.PP}</color>pp";
            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;

            result = "<size=100%>" + entry.userName + formattedAcc + formattedCombo + formattedPP + formattedMods + "</size>";

            return new ScoreData(score, result, entry.rank, false);
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

    internal class ButtonHolder
    {
        private int index;
        private Action<LeaderboardData.LeaderboardEntry> onClick;

        public ButtonHolder(int index, Action<LeaderboardData.LeaderboardEntry> endmylife)
        {
            this.index = index;
            onClick = endmylife;
        }

        [UIComponent("infoButton")]
        public Button infoButton;

        [UIAction("infoClick")]
        private void infoClick() => onClick?.Invoke(LeaderboardView.buttonEntryArray[index]);
    }
}
