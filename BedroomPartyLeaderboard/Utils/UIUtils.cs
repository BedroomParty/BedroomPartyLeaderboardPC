using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BedroomPartyLeaderboard.UI.Leaderboard;
using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;
using Tweening;
namespace BedroomPartyLeaderboard.Utils
{
    internal class UIUtils
    {
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly TweeningService _tweeningService;
        [Inject] private readonly AuthenticationManager _authenticationManager;
        [Inject] private readonly SiraLog _log;

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
            _leaderboardView._ImageHolders.ForEach(holder => holder.loadingIndicator.SetActive(true));
        }

        internal void ByeIMGLoader()
        {
            _leaderboardView._ImageHolders.ForEach(holder => holder.loadingIndicator.SetActive(false));
        }

        public void SetProfiles(List<LeaderboardData.LeaderboardEntry> leaderboard, CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0; i < leaderboard.Count; i++)
                {
                    if (leaderboard[i] == null || leaderboard[i].userName == "null")
                    {
                        _leaderboardView._ImageHolders[i].profileImage.sprite = null;
                        _leaderboardView._ImageHolders[i].loadingIndicator.gameObject.SetActive(false);
                        return;
                    }
                    _leaderboardView._ImageHolders[i].profileImage.gameObject.SetActive(true);
                    _leaderboardView._ImageHolders[i].setProfileImage($"http://api.thebedroom.party/user/{leaderboard[i].userID}/avatar", cancellationToken);
                }
                for (int i = leaderboard.Count; i <= 10; i++)
                {
                    _leaderboardView._ImageHolders[i].loadingIndicator.gameObject.SetActive(false);
                    _leaderboardView._ImageHolders[i].profileImage.sprite = null;
                }
            }
            catch (OperationCanceledException)
            {
                for (int i = leaderboard.Count; i <= 10; i++)
                {
                    _leaderboardView._ImageHolders[i].loadingIndicator.gameObject.SetActive(false);
                    _leaderboardView._ImageHolders[i].profileImage.sprite = null;
                }
            }

        }

        internal static IEnumerator GetSpriteAvatar(string url, Action<Sprite, string> onSuccess, Action<string, string> onFailure, CancellationToken cancellationToken)
        {
            var handler = new DownloadHandlerTexture();
            var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            www.downloadHandler = handler;
            cancellationToken.ThrowIfCancellationRequested();
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
            if (cool == null) _log.Error("Material 'UINoGlowRoundEdge' not found.");
            return cool;
        }

        private bool obtainedAnchor = false;
        private Vector2 normalAnchor = Vector2.zero;
        public void RichMyText(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                cell.showSeparator = true;
                TextMeshProUGUI nameText = cell.Get<TextMeshProUGUI>("_playerNameText");
                TextMeshProUGUI rankText = cell.Get<TextMeshProUGUI>("_rankText");
                TextMeshProUGUI scoreText = cell.Get<TextMeshProUGUI>("_scoreText");
                nameText.richText = true;
                rankText.richText = true;
                scoreText.richText = true;
                rankText.text = $"<size=120%><u>{rankText.text}</u></size>";
                ImageView seperator = cell.Get<Image>("_separatorImage") as ImageView;
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
                EntryHolder buttonHolder = _leaderboardView.EntryHolders[cell.idx];
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

        public static class MonoBehaviourAttacher
        {
            public static TextHoverEffect AttachTextHoverEffect(GameObject gameObject, bool shouldChangeText = false, string oldText = "", string newText = "", FontStyles daStyle = FontStyles.Normal, FontStyles origStyle = FontStyles.Normal)
            {
                TextHoverEffect textHoverEffect = gameObject.GetComponent<TextHoverEffect>() ?? gameObject.AddComponent<TextHoverEffect>();
                textHoverEffect.daComponent = gameObject.GetComponent<TextMeshProUGUI>();
                textHoverEffect.shouldChangeText = shouldChangeText;
                textHoverEffect.oldText = oldText;
                textHoverEffect.newText = newText;
                textHoverEffect.daStyle = daStyle;
                textHoverEffect.origStyle = origStyle;
                return textHoverEffect;
            }
        }


        internal class ImageHolder
        {
            private readonly int index;

            public bool isLoading = false;


            internal Sprite nullSprite = BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;

            public ImageHolder(int index)
            {
                this.index = index;
            }

            [UIComponent("profileImage")]
            public ImageView profileImage = null;

            [UIObject("loadingIndicator")]
            public GameObject loadingIndicator = null;

            [UIAction("#post-parse")]
            public void Parsed()
            {
                profileImage.sprite = nullSprite;
                profileImage.gameObject.SetActive(true);
                loadingIndicator.gameObject.SetActive(false);
            }

            public void setProfileImage(string url, CancellationToken cancellationToken)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (SpriteCache.cachedSprites.ContainsKey(url))
                    {
                        profileImage.gameObject.SetActive(true);
                        profileImage.sprite = SpriteCache.cachedSprites[url];
                        loadingIndicator.gameObject.SetActive(false);
                        return;
                    }
                    loadingIndicator.gameObject.SetActive(true);
                    SharedCoroutineStarter.instance.StartCoroutine(GetSpriteAvatar(url, OnAvatarDownloadSuccess, OnAvatarDownloadFailure, cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    OnAvatarDownloadFailure("Cancelled", cancellationToken);
                }
                finally
                {
                    SpriteCache.MaintainSpriteCache();
                }
            }

            internal static IEnumerator GetSpriteAvatar(string url, Action<Sprite, string, CancellationToken> onSuccess, Action<string, CancellationToken> onFailure, CancellationToken cancellationToken)
            {
                var handler = new DownloadHandlerTexture();
                var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
                www.downloadHandler = handler;
                yield return www.SendWebRequest();

                while (!www.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        onFailure?.Invoke("Cancelled", cancellationToken);
                        yield break;
                    }
                    yield return null;
                }
                if (www.isNetworkError || www.isHttpError)
                {
                    onFailure?.Invoke(www.error, cancellationToken);
                    yield break;
                }
                if (!string.IsNullOrEmpty(www.error))
                {
                    onFailure?.Invoke(www.error, cancellationToken);
                    yield break;
                }

                Sprite sprite = Sprite.Create(handler.texture, new Rect(0, 0, handler.texture.width, handler.texture.height), Vector2.one * 0.5f);
                onSuccess?.Invoke(sprite, url, cancellationToken);
                yield break;
            }

            internal void OnAvatarDownloadSuccess(Sprite a, string url, CancellationToken cancellationToken)
            {
                SpriteCache.AddSpriteToCache(url, a);
                if (cancellationToken != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                profileImage.gameObject.SetActive(true);
                profileImage.sprite = a;
                loadingIndicator.gameObject.SetActive(false);
            }

            internal void OnAvatarDownloadFailure(string error, CancellationToken cancellationToken)
            {
                if (cancellationToken != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                ClearSprite();
            }

            public void ClearSprite()
            {
                if (profileImage != null)
                {
                    profileImage.sprite = nullSprite;
                }
                if (loadingIndicator != null)
                {
                    loadingIndicator.gameObject.SetActive(false);
                }
            }
        }

        internal static class SpriteCache
        {
            internal static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
            private static int MaxSpriteCacheSize = 150;
            internal static Queue<string> spriteCacheQueue = new Queue<string>();
            internal static void MaintainSpriteCache()
            {
                while (cachedSprites.Count > MaxSpriteCacheSize)
                {
                    string oldestUrl = spriteCacheQueue.Dequeue();
                    cachedSprites.Remove(oldestUrl);
                }
            }

            internal static void AddSpriteToCache(string url, Sprite sprite)
            {
                if (cachedSprites.ContainsKey(url))
                {
                    return;
                }
                cachedSprites.Add(url, sprite);
                spriteCacheQueue.Enqueue(url);
            }
        }

        internal class EntryHolder
        {
            public int index;
            public Action<LeaderboardData.LeaderboardEntry> onClick;

            public EntryHolder(int index, Action<LeaderboardData.LeaderboardEntry> endmylife)
            {
                this.index = index;
                onClick = endmylife;
            }

            public void infoClick()
            {
                onClick?.Invoke(LeaderboardView.lbEntryArray[index]);
            }
        }


        internal class TweeningService
        {
            [Inject] private Tweening.TimeTweeningManager _tweeningManager;
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
