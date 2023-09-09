using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BedroomPartyLeaderboard.UI.Leaderboard;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class UIUtils
    {
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly TweeningService _tweeningService;
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
                if (leaderboard[i] == null || leaderboard[i].userName == "null")
                {
                    _leaderboardView._ImageHolders[i].profileImage.sprite = null;
                    _leaderboardView._ImageHolders[i].profileloading.gameObject.SetActive(false);
                    return;
                }
                _leaderboardView._ImageHolders[i].profileImage.gameObject.SetActive(true);
                _leaderboardView._ImageHolders[i].setProfileImage($"https://api.thebedroom.party/user/{leaderboard[i].userID}/avatar");
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
            _leaderboardView.scoreInfoModal.profileImageModal.material = mat;
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

                cell.interactable = true;
                ButtonHolder buttonHolder = _leaderboardView.Buttonholders[cell.idx];
                CellClicker clicky = cell.gameObject.AddComponent<CellClicker>();
                clicky.onClick = buttonHolder.infoClick;
                clicky.index = cell.idx;
                clicky.seperator = seperator;
                if (cell.gameObject.activeSelf && _leaderboardView.leaderboardTransform.gameObject.activeSelf)
                {
                    _tweeningService.FadeText(nameText, true, 0.3f);
                    _tweeningService.FadeText(rankText, true, 0.3f);
                    _tweeningService.FadeText(scoreText, true, 0.3f);
                }
                /*
                if (cell.idx == 9)
                {
                    ImageView silly = GameObject.Instantiate<Image>(seperator, _leaderboardView.leaderboardTransform) as ImageView;
                    silly.transform.position = seperator.transform.position;
                    silly.transform.localPosition = seperator.transform.localPosition;
                    silly.transform.localPosition += new Vector3(0, -0.1f, 0);

                    return;
                }
                */
            }
        }

        public class CellClicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
        {
            public Action onClick;
            public int index;
            public ImageView seperator;
            private Vector3 originalScale;
            private bool isScaled = false;

            private void Start()
            {
                originalScale = seperator.transform.localScale;
            }

            public void OnPointerClick(PointerEventData data)
            {
                onClick();
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!isScaled)
                {
                    seperator.transform.localScale = originalScale * 1.8f;
                    isScaled = true;
                }
                seperator.color = Color.white;
                seperator.color0 = Color.white;
                seperator.color1 = new Color(1, 1, 1, 0);
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (isScaled)
                {
                    seperator.transform.localScale = originalScale;
                    isScaled = false;
                }
                seperator.color = Constants.BP_COLOR2;
                seperator.color0 = Color.white;
                seperator.color1 = new Color(1, 1, 1, 0);
            }

            private void OnDestroy()
            {
                onClick = null;
            }
        }

        public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            public TextMeshProUGUI daComponent;
            private bool isScaled;
            public FontStyles daStyle;
            public FontStyles origStyle;

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!isScaled)
                {
                    daComponent.fontStyle = daStyle;
                    isScaled = true;
                }
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (isScaled)
                {
                    daComponent.fontStyle = origStyle;
                    isScaled = false;
                }
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
            public int index;
            public Action<LeaderboardData.LeaderboardEntry> onClick;

            public ButtonHolder(int index, Action<LeaderboardData.LeaderboardEntry> endmylife)
            {
                this.index = index;
                onClick = endmylife;
            }

            [UIComponent("infoButton")]
            public Button infoButton;

            [UIAction("infoClick")]
            public void infoClick()
            {
                onClick?.Invoke(LeaderboardView.buttonEntryArray[index]);
            }
        }


        internal class TweeningService
        {
            [Inject] private TimeTweeningManager _tweeningManager;
            private HashSet<Transform> activeRotationTweens = new HashSet<Transform>();

            public void RotateTransform(Transform transform, float rotationAmount, float time, Action callback = null)
            {
                if (activeRotationTweens.Contains(transform)) return;
                float startRotation = transform.rotation.eulerAngles.z;
                float endRotation = startRotation + rotationAmount;

                Tween tween = new FloatTween(startRotation, endRotation, (float u) =>
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, u);
                }, 0.1f, EaseType.Linear, 0f);
                tween.onCompleted = () =>
                {
                    callback?.Invoke();
                    activeRotationTweens.Remove(transform);
                };
                tween.onKilled = () =>
                {
                    if (transform != null) transform.rotation = Quaternion.Euler(0f, 0f, endRotation);
                    callback?.Invoke();
                    activeRotationTweens.Remove(transform);
                };
                activeRotationTweens.Add(transform);
                _tweeningManager.AddTween(tween, transform);
            }

            public void FadeText(TextMeshProUGUI text, bool fadeIn, float time)
            {
                float startAlpha = fadeIn ? 0f : 1f;
                float endAlpha = fadeIn ? 1f : 0f;

                Tween tween = new FloatTween(startAlpha, endAlpha, (float u) =>
                {
                    text.color = text.color.ColorWithAlpha(u);
                }, 0.4f, EaseType.Linear, 0f);
                tween.onCompleted = () =>
                {
                    if (text == null) return;
                    text.gameObject.SetActive(fadeIn);
                };
                tween.onKilled = () =>
                {
                    if (text == null) return;
                    text.gameObject.SetActive(fadeIn);
                    text.color = text.color.ColorWithAlpha(endAlpha);
                };
                text.gameObject.SetActive(true);
                _tweeningManager.AddTween(tween, text);
            }
        }
    }
}
