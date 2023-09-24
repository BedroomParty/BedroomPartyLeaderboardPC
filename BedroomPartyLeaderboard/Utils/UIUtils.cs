using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BedroomPartyLeaderboard.UI.Leaderboard;
using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class UIUtils
    {
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly TweeningService _tweeningService;
        [Inject] private readonly AuthenticationManager _authenticationManager;
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

        internal void ByeImages()
        {
            _leaderboardView._ImageHolders.ForEach(holder => holder.profileImage.sprite = null);
        }

        internal void HelloIMGLoader()
        {
            _leaderboardView._ImageHolders.ForEach(holder => holder.profileloading.SetActive(true));
        }

        internal void ByeIMGLoader()
        {
            _leaderboardView._ImageHolders.ForEach(holder => holder.profileloading.SetActive(false));
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
                _leaderboardView._ImageHolders[i].setProfileImage($"http://api.thebedroom.party/user/{leaderboard[i].userID}/avatar");
            }

            for (int i = leaderboard.Count; i <= 10; i++)
            {
                _leaderboardView._ImageHolders[i].profileloading.gameObject.SetActive(false);
                _leaderboardView._ImageHolders[i].profileImage.sprite = null;
            }
        }

        internal static IEnumerator GetSpriteAvatar(string url, Action<Sprite, string> onSuccess, Action<string, string> onFailure, CancellationToken cancellationToken)
        {
            var handler = new DownloadHandlerTexture();
            var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            www.downloadHandler = handler;
            yield return www.SendWebRequest();

            while (!www.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    onFailure?.Invoke("Cancelled", url);
                    yield break;
                }

                yield return null;
            }
            if (www.isHttpError || www.isNetworkError)
            {
                onFailure?.Invoke(www.error, url);
                yield break;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                onFailure?.Invoke(www.error, url);
                yield break;
            }
            Sprite sprite = Sprite.Create(handler.texture, new Rect(0, 0, handler.texture.width, handler.texture.height), Vector2.one * 0.5f);
            onSuccess?.Invoke(sprite, url);
        }

        public async Task SetToast(string text, bool fullyActive, bool loadingActive, int delayMilliseconds)
        {
            await Constants.WaitUntil(() => _panelView.promptText != null && _panelView.prompt_loader != null);

            if (!fullyActive)
            {
                _panelView.prompt_loader.SetActive(false);
                _panelView.promptText.gameObject.SetActive(false);
                return;
            }

            if (loadingActive)
            {
                _panelView.prompt_loader.SetActive(true);
            }
            else
            {
                _panelView.prompt_loader.SetActive(false);
            }

            _panelView.promptText.gameObject.SetActive(true);
            _tweeningService.FadeText(_panelView.promptText, true, 0.20f);

            _panelView.promptText.text = text;

            if (delayMilliseconds > 0)
            {
                await Task.Delay(delayMilliseconds);
                _panelView.prompt_loader.SetActive(false);
                _tweeningService.FadeText(_panelView.promptText, false, 0.15f);
            }
        }



        public async Task assignStaff()
        {
            if (await Task.Run(() => Constants.isStaff(_authenticationManager._localPlayerInfo.userID).Result))
            {
                RainbowAnimation rainbowAnimation = _panelView.playerUsername.gameObject.AddComponent<RainbowAnimation>();
                rainbowAnimation.speed = 0.35f;
            }
            else
            {
                RainbowAnimation rainbowAnimation = _panelView.playerUsername.gameObject.GetComponent<RainbowAnimation>();
                if (rainbowAnimation != null)
                {
                    UnityEngine.Object.Destroy(rainbowAnimation);
                }
                _panelView.playerUsername.color = Color.white;
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
            cool = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UINoGlowRoundEdge");
            if (cool == null) Plugin.Log.Error("Material 'UINoGlowRoundEdge' not found.");
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
                    _tweeningService.FadeText(nameText, true, 0.2f);
                    _tweeningService.FadeText(rankText, true, 0.2f);
                    _tweeningService.FadeText(scoreText, true, 0.2f);
                }
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
            public bool shouldChangeText = false;
            public string oldText = "";
            public string newText = "";
            public FontStyles daStyle = FontStyles.Normal;
            public FontStyles origStyle = FontStyles.Normal;

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!isScaled)
                {
                    if (shouldChangeText)
                    {
                        daComponent.text = newText;
                    }
                    daComponent.fontStyle = daStyle;
                    isScaled = true;
                }
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (isScaled)
                {
                    if (shouldChangeText)
                    {
                        daComponent.text = oldText;
                    }
                    daComponent.fontStyle = origStyle;
                    isScaled = false;
                }
            }
        }

        public class ImageHolder
        {
            private readonly int index;

            public bool isLoading;

            private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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
                profileloading.gameObject.SetActive(true);
                if (SpriteCache.TryGetSprite(url, out Sprite sprite))
                {
                    profileImage.sprite = sprite;
                    profileloading.gameObject.SetActive(false);
                    isLoading = false;
                    return;
                }
                try
                {
                    if (isLoading)
                    {
                        CancelDownload();
                    }

                    isLoading = true;
                    profileloading.SetActive(true);

                    cancellationTokenSource = new CancellationTokenSource();

                    Task.Run(() => ThisFuckingSucks(url));
                }
                catch
                {
                    isLoading = false;
                }
            }

            private async Task ThisFuckingSucks(string url)
            {
                await Constants.WaitUntil(() => profileImage.IsActive());
                UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    profileImage.StartCoroutine(GetSpriteAvatar(url, OnAvatarYay, OnAvatarNay, cancellationTokenSource.Token));
                });
            }

            private void OnAvatarYay(Sprite a, string url)
            {
                profileImage.sprite = a;
                profileloading.gameObject.SetActive(false);
                isLoading = false;
                SpriteCache.AddSprite(url, a);
            }

            private void OnAvatarNay(string a, string url)
            {
                profileImage.sprite = Utilities.FindSpriteInAssembly("BedroomPartyLeaderboard.Images.Player.png");
                profileloading.gameObject.SetActive(false);
                isLoading = false;
            }

            private void CancelDownload()
            {
                if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
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

        public static class SpriteCache
        {
            private static Dictionary<string, Sprite> spriteDictionary = new();
            private static Sprite[] sprites;

            public static void AddSprite(string spriteName, Sprite sprite)
            {
                if (!spriteDictionary.ContainsKey(spriteName))
                {
                    spriteDictionary[spriteName] = sprite;
                }
            }

            public static bool TryGetSprite(string spriteName, out Sprite sprite)
            {
                if (spriteDictionary.TryGetValue(spriteName, out sprite))
                {
                    return true;
                }
                else
                {
                    sprite = null;
                    return false;
                }
            }
        }
    }
}
