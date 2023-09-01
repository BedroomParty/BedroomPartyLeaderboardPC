using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BedroomPartyLeaderboard.UI.Leaderboard;
using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class UIUtils
    {
        [Inject] private PanelView _panelView;
        [Inject] private LeaderboardView _leaderboardView;

        public async Task SetProfilePic(ImageView image, string url)
        {
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
                Texture2D roundedTexture = RoundTextureCorners(texture);
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

        public async void SetProfileImage(string url, int index, string userID)
        {
            ImageView image = _leaderboardView._ImageHolders[index].profileImage;
            GameObject loader = _leaderboardView._ImageHolders[index].profileloading;

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Delay(10);
            }

            if (request.responseCode == 200)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Texture2D roundedTexture = RoundTextureCorners(texture);
                Sprite sprite = Sprite.Create(roundedTexture, new Rect(0, 0, roundedTexture.width, roundedTexture.height), Vector2.one * 0.5f);

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

        public Texture2D RoundTextureCorners(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            Texture2D roundedTexture = new Texture2D(width, height);
            Color[] pixels = texture.GetPixels();

            float cornerRadius = Mathf.Min(width, height) * 0.5f;

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

        public void SetProfiles(List<LeaderboardData.LeaderboardEntry> leaderboard)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                SetProfileImage(Constants.profilePictureLink(leaderboard[i].userID), i, leaderboard[i].userID);
            }

            for (int i = leaderboard.Count; i <= 10; i++)
            {
                _leaderboardView._ImageHolders[i].profileloading.gameObject.SetActive(false);
            }
        }

        public void RichMyText(LeaderboardTableView tableView)
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
                seperator.color = Constants.BP_COLOR2;
                seperator.color0 = Color.white;
                seperator.color1 = new Color(1, 1, 1, 0);
            }
        }
        public class ImageHolder
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

            public void setProfileImage(string url)
            {
                profileloading.SetActive(true);
                profileImage.SetImage(url);
                profileloading.SetActive(false);
            }
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
}
