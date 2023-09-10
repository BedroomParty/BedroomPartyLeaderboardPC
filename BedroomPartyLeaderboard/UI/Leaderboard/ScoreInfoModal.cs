using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BedroomPartyLeaderboard.UI.Leaderboard;
using BedroomPartyLeaderboard.Utils;
using HMUI;
using IPA.Utilities.Async;
using ModestTree;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using static BedroomPartyLeaderboard.Utils.UIUtils;

namespace BedroomPartyLeaderboard.UI
{
    internal class ScoreInfoModal
    {
        [UIComponent("scoreInfo")]
        public ModalView infoModal;

        [UIComponent("usernameScoreText")]
        private readonly ClickableText usernameScoreText;

        [UIComponent("dateScoreText")]
        private readonly TextMeshProUGUI dateScoreText;

        [UIComponent("accScoreText")]
        private readonly TextMeshProUGUI accScoreText;

        [UIComponent("scoreScoreText")]
        private readonly TextMeshProUGUI scoreScoreText;

        [UIComponent("fcScoreText")]
        private readonly TextMeshProUGUI fcScoreText;

        [UIComponent("maxComboScoreText")]
        private readonly TextMeshProUGUI maxComboScoreText;

        [UIComponent("modifiersScoreText")]
        private readonly TextMeshProUGUI modifiersScoreText;

        [UIComponent("ppScoreText")]
        private readonly TextMeshProUGUI ppScoreText;

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

        private LeaderboardData.LeaderboardEntry currentEntry;
        private const int scoreDetails = 4;
        private const float infoFontSize = 4.2f;

        public void setScoreModalText(LeaderboardData.LeaderboardEntry entry)
        {
            currentEntry = entry;
            profileImageModalLOADING.SetActive(true);
            TimeSpan relativeTime = TimeUtils.GetRelativeTime(entry.timestamp.ToString());
            dateScoreText.text = string.Format("<size=4.8><color=white>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));

            usernameScoreText.text = $"<size=180%>{entry.userName}</color>";
            usernameScoreText.richText = true;

            accScoreText.text = $"Accuracy: <size={infoFontSize}><color=#ffd42a>{entry.acc:F2}%</color></size>";
            scoreScoreText.text = $"Score: <size={infoFontSize}>{entry.modifiedScore:N0}</size>";
            scoreScoreText.text.Replace(",", " ");
            modifiersScoreText.text = $"Mods: <size=4.4>{entry.mods}</size>";

            ppScoreText.gameObject.SetActive(false);

            if (entry.mods.IsEmpty())
            {
                modifiersScoreText.gameObject.SetActive(false);
            }
            else
            {
                modifiersScoreText.gameObject.SetActive(true);
            }

            fcScoreText.text = entry.fullCombo
                ? "<size=4><color=green>Full Combo!</color></size>"
                : string.Format("Mistakes: <size=4><color=red>{0}</color></size>", entry.badCutCount + entry.missCount);
            parserParams.EmitEvent("showScoreInfo");

            UnityMainThreadTaskScheduler.Factory.StartNew(() =>
            {
                profileImageModal.SetImage($"https://api.thebedroom.party/user/{entry.userID}/avatar");
                profileImageModalLOADING.SetActive(false);

                if (Task.Run(() => Constants.isStaff(entry.userID)).Result)
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

                if (usernameScoreText.gameObject.GetComponent<UIUtils.TextHoverEffect>() != null)
                {
                    UnityEngine.Object.Destroy(usernameScoreText.gameObject.GetComponent<UIUtils.TextHoverEffect>());
                }
                TextHoverEffect textHoverEffect = usernameScoreText.gameObject.AddComponent<UIUtils.TextHoverEffect>();
                textHoverEffect.daComponent = usernameScoreText;
                textHoverEffect.daStyle = FontStyles.Underline;
                textHoverEffect.origStyle = FontStyles.Normal;

            });
        }
    }
}
