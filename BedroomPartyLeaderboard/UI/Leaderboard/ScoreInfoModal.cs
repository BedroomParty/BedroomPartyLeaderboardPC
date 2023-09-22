using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Tags;
using BedroomPartyLeaderboard.UI.Leaderboard;
using BedroomPartyLeaderboard.Utils;
using HMUI;
using IPA.Utilities.Async;
using ModestTree;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static BedroomPartyLeaderboard.Utils.UIUtils;

namespace BedroomPartyLeaderboard.UI
{
    internal class ScoreInfoModal
    {
        [Inject] private readonly LeaderboardView _leaderboardView;

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

        [UIObject("normalModalInfo")]
        private readonly GameObject normalModalInfo;

        [UIObject("moreModalInfo")]
        private readonly GameObject moreModalInfo;

        [UIComponent("moreInfoButton")]
        private readonly Button moreInfoButton;

        [UIComponent("profileImageModal")]
        public ImageView profileImageModal;

        [UIObject("profileImageModalLOADING")]
        public GameObject profileImageModalLOADING;

        [UIComponent("avgHandAccLeft")]
        private readonly TextMeshProUGUI avgHandAccLeft;

        [UIComponent("avgHandAccRight")]
        private readonly TextMeshProUGUI avgHandAccRight;

        [UIComponent("avgHandTDLeft")]
        private readonly TextMeshProUGUI avgHandTDLeft;

        [UIComponent("avgHandTDRight")]
        private readonly TextMeshProUGUI avgHandTDRight;

        [UIComponent("pauses")]
        private readonly TextMeshProUGUI pauses;

        [UIComponent("perfectStreak")]
        private readonly TextMeshProUGUI perfectStreak;

        [UIParams]
        public BSMLParserParams parserParams;

        [UIAction("usernameScoreTextCLICK")]
        public void usernameScoreTextCLICK()
        {
            Application.OpenURL(Constants.USER_URL_WEB(currentEntry.userID));
        }

        public void hidethefucker()
        {
            parserParams.EmitEvent("hideScoreInfo");
        }

        private bool isMoreInfo = false;

        [UIAction("moreInfoButtonCLICK")]
        public void moreInfoButtonCLICK()
        {
            isMoreInfo = !isMoreInfo;
            if (isMoreInfo)
            {
                moreInfoButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
                moreModalInfo.SetActive(true);
                normalModalInfo.SetActive(false);
            }
            else
            {
                moreInfoButton.GetComponentInChildren<TextMeshProUGUI>().text = "More Info";
                moreModalInfo.SetActive(false);
                normalModalInfo.SetActive(true);
            }
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

            usernameScoreText.text = $"{entry.userName}";
            usernameScoreText.richText = true;

            isMoreInfo = false;
            moreInfoButton.GetComponentInChildren<TextMeshProUGUI>().text = "More Info";
            moreModalInfo.SetActive(false);
            normalModalInfo.SetActive(true);

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

            fcScoreText.text = (bool)entry.fullCombo
                ? $"<size=4><color={Constants.goodToast}>Full Combo!</color></size>"
                : $"<size=4><color={Constants.badToast}>Mistakes: {entry.badCutCount + entry.missCount}</color></size>";

            avgHandAccLeft.text = entry.avgHandAccLeft.HasValue ? $"Left Hand Acc: <size={infoFontSize}><color=#ffd42a>{entry.avgHandAccLeft:F2}</color></size>" : "";
            avgHandAccRight.text = entry.avgHandAccRight.HasValue ? $"Right Hand Acc: <size={infoFontSize}><color=#ffd42a>{entry.avgHandAccRight:F2}</color></size>" : "";
            avgHandTDLeft.text = entry.avgHandTDLeft.HasValue ? $"Left Hand TD: <size={infoFontSize}><color=#ffd42a>{entry.avgHandTDLeft:F2}</color></size>" : "";
            avgHandTDRight.text = entry.avgHandTDRight.HasValue ? $"Right Hand TD: <size={infoFontSize}><color=#ffd42a>{entry.avgHandTDRight:F2}</color></size>" : "";
            pauses.text = entry.pauses.HasValue ? $"Pauses: <size={infoFontSize}><color=#ffd42a>{entry.pauses}</color></size>" : "";
            perfectStreak.text = entry.perfectStreak.HasValue ? $"Perfect Streak: <size={infoFontSize}><color=#ffd42a>{entry.perfectStreak}</color></size>" : "";

            parserParams.EmitEvent("showScoreInfo");
            parserParams.EmitEvent("hideSeasonSelectModal");
            parserParams.EmitEvent("hideInfoModal");

            UnityMainThreadTaskScheduler.Factory.StartNew(() =>
            {
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

                Task.Run(() =>
                {
                    while (_leaderboardView._ImageHolders[(int)currentEntry.rank - 1].isLoading)
                    {

                    }

                    UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        profileImageModal.sprite = _leaderboardView._ImageHolders[(int)currentEntry.rank - 1].profileImage.sprite;
                        profileImageModalLOADING.SetActive(false);
                    });
                });
            });
        }
    }
}
