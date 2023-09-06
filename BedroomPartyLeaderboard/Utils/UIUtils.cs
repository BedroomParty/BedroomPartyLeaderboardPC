using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BedroomPartyLeaderboard.UI.Leaderboard;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class UIUtils
    {
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly LeaderboardView _leaderboardView;

        public class RainbowAnimation : MonoBehaviour
        {
            public float speed = 1f; // Speed of the color change

            private ClickableText clickableText;
            private float hue;

            private void Start()
            {
                clickableText = GetComponent<ClickableText>();
            }

            private void Update()
            {
                if (clickableText == null)
                {
                    return;
                }

                hue += speed * Time.deltaTime;
                if (hue > 1f)
                {
                    hue -= 1f;
                }

                Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
                clickableText.color = rainbowColor;
            }
        }

        public void SetProfiles(List<LeaderboardData.LeaderboardEntry> leaderboard)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                _leaderboardView._ImageHolders[i].profileImage.gameObject.SetActive(true);
                _leaderboardView._ImageHolders[i].setProfileImage($"https://api.thebedroom/party/{leaderboard[i].userID}/avatar");
            }

            for (int i = leaderboard.Count; i <= 10; i++)
            {
                _leaderboardView._ImageHolders[i].profileloading.gameObject.SetActive(false);
                _leaderboardView._ImageHolders[i].profileImage.sprite = null;
            }
        }

        public void GetCoolMaterialAndApply()
        {
            Material mat = FindCoolMaterial();
            foreach (ImageHolder x in _leaderboardView._ImageHolders)
            {
                x.profileImage.material = mat;
            }
            _panelView.playerAvatar.material = mat;
        }

        private Material FindCoolMaterial()
        {
            Material cool = null;
            foreach (Material material in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (material == null)
                {
                    continue;
                }

                if (material.name.Contains("UINoGlowRoundEdge"))
                {
                    cool = material;
                    break;
                }
            }

            if (cool == null)
            {
                Plugin.Log.Error("Material 'UINoGlowRoundEdge' not found.");
            }

            return cool;
        }

        private bool obtainedAnchor = false;
        private Vector2 normalAnchor = Vector2.zero;
        public void RichMyText(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                cell.showSeparator = true;
                TextMeshProUGUI nameText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");
                TextMeshProUGUI rankText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText");
                TextMeshProUGUI scoreText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText");
                nameText.richText = true;
                rankText.richText = true;
                scoreText.richText = true;
                rankText.text = $"<size=120%><u>{rankText.text}</u></size>";
                ImageView seperator = cell.GetField<Image, LeaderboardTableCell>("_separatorImage") as ImageView;
                seperator.color = Constants.BP_COLOR2;
                seperator.color0 = Color.white;
                seperator.color1 = new Color(1, 1, 1, 0);
                if (!obtainedAnchor)
                {
                    normalAnchor = nameText.rectTransform.anchoredPosition;
                    obtainedAnchor = true;
                }
                Vector2 newPosition = new(normalAnchor.x + 2.5f, 0f);
                nameText.rectTransform.anchoredPosition = newPosition;
            }
        }
        public class ImageHolder
        {
            private readonly int index;

            public bool isLoading;

            public ImageHolder(int index)
            {
                this.index = index;
            }

            [UIComponent("profileImage")]
            public ImageView profileImage;

            [UIObject("profileloading")]
            public GameObject profileloading;

            public void setProfileImage(string url)
            {
                isLoading = true;
                profileloading.SetActive(true);
                profileImage.SetImage(url);
                profileloading.SetActive(false);
                isLoading = false;
            }
        }

        internal class ButtonHolder
        {
            private readonly int index;
            private readonly Action<LeaderboardData.LeaderboardEntry> onClick;

            public ButtonHolder(int index, Action<LeaderboardData.LeaderboardEntry> endmylife)
            {
                this.index = index;
                onClick = endmylife;
            }

            [UIComponent("infoButton")]
            public Button infoButton;

            [UIAction("infoClick")]
            private void infoClick()
            {
                onClick?.Invoke(LeaderboardView.buttonEntryArray[index]);
            }
        }
    }
}
