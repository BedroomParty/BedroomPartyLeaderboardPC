using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities.Async;
using ModestTree;
using QSLeaderboard.UI.Leaderboard;
using QSLeaderboard.Utils;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace QSLeaderboard.UI
{
    internal class ScoreInfoModal
    {
        [UIComponent("scoreInfo")]
        public ModalView infoModal;

        [UIComponent("usernameScoreText")]
        private ClickableText usernameScoreText;

        [UIComponent("dateScoreText")]
        private TextMeshProUGUI dateScoreText;

        [UIComponent("accScoreText")]
        private TextMeshProUGUI accScoreText;

        [UIComponent("scoreScoreText")]
        private TextMeshProUGUI scoreScoreText;

        [UIComponent("fcScoreText")]
        private TextMeshProUGUI fcScoreText;

        [UIComponent("maxComboScoreText")]
        private TextMeshProUGUI maxComboScoreText;

        [UIComponent("modifiersScoreText")]
        private TextMeshProUGUI modifiersScoreText;

        [UIComponent("ppScoreText")]
        private TextMeshProUGUI ppScoreText;

        [UIComponent("profileImageModal")]
        public ImageView profileImageModal;

        [UIObject("profileImageModalLOADING")]
        public GameObject profileImageModalLOADING;

        [UIParams]
        public BSMLParserParams parserParams;

        [UIAction("usernameScoreTextCLICK")]
        public void usernameScoreTextCLICK()
        {
            Application.OpenURL(Constants.USER_PROFILE_LINK + currentEntry.userID);
        }

        [Inject] LeaderboardView _leaderboardView;

        LeaderboardData.LeaderboardEntry currentEntry;

        const int scoreDetails = 4;

        const float infoFontSize = 4.2f;

        public void setScoreModalText(LeaderboardData.LeaderboardEntry entry)
        {
            currentEntry = entry;

            string formattedDate = "Error";
            profileImageModalLOADING.SetActive(true);

            TimeSpan relativeTime = TimeUtils.GetRelativeTime(entry.timestamp.ToString());
            dateScoreText.text = string.Format("<size=4.8><color=white>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));

            usernameScoreText.text = $"<size=180%>{entry.userName}</color>";
            usernameScoreText.richText = true;

            accScoreText.text = $"Accuracy: <size={infoFontSize}><color=#ffd42a>{entry.acc.ToString("F2")}%</color></size>";
            scoreScoreText.text = $"Score: <size={infoFontSize}>{entry.score.ToString("N0")}</size>";
            scoreScoreText.text.Replace(",", " ");
            modifiersScoreText.text = $"Mods: <size=4.4>{entry.mods}</size>";

            if (entry.PP != 0)
            {
                ppScoreText.text = string.Format("<size=4.8><color=#BCE59C>{0}<size=3>pp</size></color></size>", entry.PP.ToString("F2"));
            }
            else
            {
                ppScoreText.gameObject.SetActive(false);
            }

            if (entry.mods.IsEmpty()) modifiersScoreText.gameObject.SetActive(false);
            else modifiersScoreText.gameObject.SetActive(true);

            if (entry.fullCombo) fcScoreText.text = "<size=4><color=green>Full Combo!</color></size>";
            else fcScoreText.text = string.Format("Mistakes: <size=4><color=red>{0}</color></size>", entry.badCutCount + entry.missCount);

            SetProfileImageModal(Constants.profilePictureLink(entry.userID), profileImageModal);
            parserParams.EmitEvent("showScoreInfo");

            if (Constants.isStaff(entry.userID))
            {
                RainbowAnimation rainbowAnimation = usernameScoreText.gameObject.AddComponent<RainbowAnimation>();
                rainbowAnimation.speed = 0.4f;
            }
            else
            {
                RainbowAnimation rainbowAnimation = usernameScoreText.GetComponent<RainbowAnimation>();
                if (rainbowAnimation != null)
                {
                    UnityEngine.Object.Destroy(rainbowAnimation);
                }
                usernameScoreText.color = Color.white;
            }
        }

        private async void SetProfileImageModal(string url, ImageView image)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Delay(10);
            }

            if (request.responseCode == 200)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Texture2D roundedTexture = _leaderboardView.RoundTextureCorners(texture);
                Sprite sprite = Sprite.Create(roundedTexture, new Rect(0, 0, roundedTexture.width, roundedTexture.height), Vector2.one * 0.5f);

                await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    image.sprite = sprite;
                    profileImageModalLOADING.SetActive(false);
                });
            }
            else
            {
                Plugin.Log.Error("Failed to retrieve profile image: " + request.error);
            }
            request.Dispose();
        }
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

}
