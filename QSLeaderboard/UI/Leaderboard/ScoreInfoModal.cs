using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities.Async;
using ModestTree;
using QSLeaderboard.UI.Leaderboard;
using QSLeaderboard.Utils;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;
using static QSLeaderboard.Utils.UIUtils;

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
            profileImageModalLOADING.SetActive(true);
            currentEntry = entry;

            string formattedDate = "Error";
            profileImageModal.sprite = _leaderboardView.transparentSprite;
            profileImageModalLOADING.SetActive(false);

            TimeSpan relativeTime = TimeUtils.GetRelativeTime(entry.timestamp.ToString());
            dateScoreText.text = string.Format("<size=4.8><color=white>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));

            usernameScoreText.text = $"<size=180%>{entry.userName}</color>";
            usernameScoreText.richText = true;

            accScoreText.text = $"Accuracy: <size={infoFontSize}><color=#ffd42a>{entry.acc.ToString("F2")}%</color></size>";
            scoreScoreText.text = $"Score: <size={infoFontSize}>{entry.score.ToString("N0")}</size>";
            scoreScoreText.text.Replace(",", " ");
            modifiersScoreText.text = $"Mods: <size=4.4>{entry.mods}</size>";

            if (entry.PP != 0) ppScoreText.text = string.Format("<size=4.8><color=#BCE59C>{0}<size=3>pp</size></color></size>", entry.PP.ToString("F2"));
            else ppScoreText.gameObject.SetActive(false);

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
            while (_leaderboardView.holders[pos].profileImage.sprite == _leaderboardView.transparentSprite)
            {
                yield return null;
            }
            image.sprite = _leaderboardView.holders[pos].profileImage.sprite;
            profileImageModalLOADING.SetActive(false);
        }
    }
}
