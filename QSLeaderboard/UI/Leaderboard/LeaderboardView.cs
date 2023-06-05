using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
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
        public int page = 0;
        public int totalPages;
        public int sortMethod;

        private IDifficultyBeatmap currentDifficultyBeatmap;

        [UIComponent("leaderboardTableView")]
        private LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        private Transform leaderboardTransform = null;

        [UIComponent("errorText")]
        private TextMeshProUGUI errorText;

        [UIComponent("up_button")]
        private Button up_button;

        [UIComponent("down_button")]
        private Button down_button;

        [UIAction("OnPageUp")]
        private void OnPageUp() => UpdatePageChanged(-1);

        [UIAction("OnPageDown")]
        private void OnPageDown() => UpdatePageChanged(1);

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

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _loadingControl = leaderboardTransform.Find("LoadingControl").gameObject;
            var loadingContainer = _loadingControl.transform.Find("LoadingContainer");
            loadingContainer.gameObject.SetActive(false);
            Destroy(loadingContainer.Find("Text").gameObject);
            Destroy(_loadingControl.transform.Find("RefreshContainer").gameObject);
            Destroy(_loadingControl.transform.Find("DownloadingContainer").gameObject);
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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!this.isActiveAndEnabled) return;
            if (!_plvc) return;
            OnLeaderboardSet(currentDifficultyBeatmap);
            if (firstActivation)
            {
                _panelView.promptText.text = "Signing In...";
                _panelView.prompt_loader.gameObject.SetActive(true);

                _playerUtils.GetAuthStatus(result =>
                {
                    bool isAuthenticated = result.Item1;
                    string username = result.Item2;

                    if (isAuthenticated)
                    {
                        Console.WriteLine("Authenticated! Username: " + username);
                        _panelView.promptText.text = $"Successfully signed {username} in!";
                        _panelView.prompt_loader.SetActive(false);
                    }
                    else
                    {
                        _panelView.promptText.text = $"Failed to Authenticate! Report to Speecil or Nuggo if you are whitelisted!";
                        _panelView.prompt_loader.SetActive(false);
                        Console.WriteLine("Not authenticated! Username: " + username);
                    }
                });
            }
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
        }
    }
}
