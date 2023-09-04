using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BedroomPartyLeaderboard.UI.Leaderboard;
using BedroomPartyLeaderboard.Utils;
using HMUI;
using IPA.Utilities.Async;
using ModestTree;
using System;
using System.Collections;
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

        [Inject] readonly LeaderboardView _leaderboardView;

        LeaderboardData.LeaderboardEntry currentEntry;

        const int scoreDetails = 4;

        const float infoFontSize = 4.2f;

        public void setScoreModalText(LeaderboardData.LeaderboardEntry entry)
        {
            currentEntry = entry;
            profileImageModalLOADING.SetActive(true);
            string formattedDate = "Error";
            TimeSpan relativeTime = TimeUtils.GetRelativeTime(entry.timestamp.ToString());
            dateScoreText.text = string.Format("<size=4.8><color=white>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));

            usernameScoreText.text = $"<size=180%>{entry.userName}</color>";
            usernameScoreText.richText = true;

            accScoreText.text = $"Accuracy: <size={infoFontSize}><color=#ffd42a>{entry.acc.ToString("F2")}%</color></size>";
            scoreScoreText.text = $"Score: <size={infoFontSize}>{entry.score.ToString("N0")}</size>";
            scoreScoreText.text.Replace(",", " ");
            modifiersScoreText.text = $"Mods: <size=4.4>{entry.mods}</size>";

            ppScoreText.gameObject.SetActive(false);

            if (entry.mods.IsEmpty()) modifiersScoreText.gameObject.SetActive(false);
            else modifiersScoreText.gameObject.SetActive(true);

            if (entry.fullCombo) fcScoreText.text = "<size=4><color=green>Full Combo!</color></size>";
            else fcScoreText.text = string.Format("Mistakes: <size=4><color=red>{0}</color></size>", entry.badCutCount + entry.missCount);
            parserParams.EmitEvent("showScoreInfo");

            UnityMainThreadTaskScheduler.Factory.StartNew(() =>
            {
                int position = (entry.rank % 10) - 1;
                _leaderboardView.StartCoroutine(SetProfileImageModal(position, profileImageModal));

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
            });
        }

        private IEnumerator SetProfileImageModal(int pos, ImageView image)
        {
            profileImageModalLOADING.gameObject.SetActive(true);
            while (!_leaderboardView._ImageHolders[pos].isLoading)
            {
                yield return null;
            }
            image.sprite = _leaderboardView._ImageHolders[pos].profileImage.sprite;
            profileImageModal.gameObject.SetActive(true);
            profileImageModalLOADING.SetActive(false);
        }
    }
}
