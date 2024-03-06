using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BloqParty.UI.Leaderboard;
using BloqParty.Utils;
using HMUI;
using IPA.Utilities.Async;
using ModestTree;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static BloqParty.Utils.UIUtils;

namespace BloqParty.UI
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

        [UIComponent("backInfoButton")]
        private readonly Button backInfoButton;

        [UIComponent("profileImageModal")]
        public ImageView profileImageModal;

        [UIObject("profileImageModalLOADING")]
        public GameObject profileImageModalLOADING;

        [UIComponent("leftHandAverageScore")]
        private readonly TextMeshProUGUI leftHandAverageScore;

        [UIComponent("rightHandAverageScore")]
        private readonly TextMeshProUGUI rightHandAverageScore;

        [UIComponent("leftHandTimeDependency")]
        private readonly TextMeshProUGUI leftHandTimeDependency;

        [UIComponent("rightHandTimeDependency")]
        private readonly TextMeshProUGUI rightHandTimeDependency;

        [UIComponent("pauses")]
        private readonly TextMeshProUGUI pauses;

        [UIComponent("perfectStreak")]
        private readonly TextMeshProUGUI perfectStreak;

        [UIParams]
        public BSMLParserParams parserParams;

        [UIAction("ScoreUsernameSelect")]
        public void ScoreUsernameSelect() => Application.OpenURL(Constants.USER_URL_WEB(currentEntry.userID));
        public void hidethefucker()
        {
            parserParams.EmitEvent("hideScoreInfo");
        }

        private bool isMoreInfo = false;

        [UIAction("MoreInfoClick")]
        public void MoreInfoClick()
        {
            isMoreInfo = !isMoreInfo;
            moreInfoButton.gameObject.SetActive(!isMoreInfo);
            backInfoButton.gameObject.SetActive(isMoreInfo);
            moreModalInfo.SetActive(isMoreInfo);
            normalModalInfo.SetActive(!isMoreInfo);
        }

        private LeaderboardData.LeaderboardEntry currentEntry;
        private const int scoreDetails = 4;
        private const float infoFontSize = 4.2f;

        public void setScoreModalText(LeaderboardData.LeaderboardEntry entry)
        {
            currentEntry = entry;
            profileImageModalLOADING.SetActive(true);
            TimeSpan relativeTime = TimeUtils.GetRelativeTime(entry.timestamp.ToString());
            dateScoreText.text = string.Format("<size=4><color=white>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));

            usernameScoreText.text = $"{entry.userName}";
            usernameScoreText.richText = true;

            isMoreInfo = false;
            backInfoButton.gameObject.SetActive(false);
            moreInfoButton.gameObject.SetActive(true);
            moreModalInfo.SetActive(false);
            normalModalInfo.SetActive(true);

            accScoreText.text = $"Accuracy: <size={infoFontSize}><color=#FF69B4>{entry.acc:0.##}%</color></size>";
            scoreScoreText.text = $"Score: <size={infoFontSize}>{entry.modifiedScore:0,0}</size>";
            modifiersScoreText.text = $"Mods: <size=4.4>{entry.mods}</size>";
            leftHandAverageScore.text = entry.leftHandAverageScore.HasValue ? $"Left Hand Acc: <size={infoFontSize}><color=#FF69B4>{entry.leftHandAverageScore:0.##}</color></size>" : "";
            rightHandAverageScore.text = entry.rightHandAverageScore.HasValue ? $"Right Hand Acc: <size={infoFontSize}><color=#FF69B4>{entry.rightHandAverageScore:0.##}</color></size>" : "";
            leftHandTimeDependency.text = entry.leftHandTimeDependency.HasValue ? $"Left Hand TD: <size={infoFontSize}><color=#FF69B4>{entry.leftHandTimeDependency:0.##}</color></size>" : "";
            rightHandTimeDependency.text = entry.rightHandTimeDependency.HasValue ? $"Right Hand TD: <size={infoFontSize}><color=#FF69B4>{entry.rightHandTimeDependency:0.##}</color></size>" : "";
            pauses.text = entry.pauses.HasValue ? $"Pauses: <size={infoFontSize}><color=#FF69B4>{entry.pauses}</color></size>" : "";
            perfectStreak.text = entry.perfectStreak.HasValue ? $"Perfect Streak: <size={infoFontSize}><color=#FF69B4>{entry.perfectStreak}</color></size>" : "";

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

            MonoBehaviourAttacher.AttachTextHoverEffect(accScoreText.gameObject, true, accScoreText.text, $"<color=#ffd42a>FC</color> Accuracy: <size={infoFontSize}><color=#FF69B4>{(entry.fcAcc ?? 0).ToString("F2")}%</color></size>");
            MonoBehaviourAttacher.AttachTextHoverEffect(leftHandAverageScore.gameObject, true, leftHandAverageScore.text, $"Left Hand Acc: <size={infoFontSize}><color=#FF69B4>{LeaderboardDataUtils.GetAccPercentFromHand(entry.leftHandAverageScore ?? 0.0f)}</color></size>");
            MonoBehaviourAttacher.AttachTextHoverEffect(rightHandAverageScore.gameObject, true, rightHandAverageScore.text, $"Right Hand Acc: <size={infoFontSize}><color=#FF69B4>{LeaderboardDataUtils.GetAccPercentFromHand(entry.rightHandAverageScore ?? 0.0f)}</color></size>");

            parserParams.EmitEvent("showScoreInfo");
            parserParams.EmitEvent("hideSeasonSelectModal");
            parserParams.EmitEvent("hideInfoModal");

            RainbowAnimation rainbowAnimation;
            if (Constants.staffIDs.Contains(entry.userID))
            {
                if (!usernameScoreText.TryGetComponent<RainbowAnimation>(out rainbowAnimation))
                {
                    rainbowAnimation = usernameScoreText.gameObject.AddComponent<RainbowAnimation>();
                    rainbowAnimation.speed = 0.4f;
                }
            }
            else
            {
                if (usernameScoreText.TryGetComponent<RainbowAnimation>(out rainbowAnimation))
                {
                    UnityEngine.Object.Destroy(rainbowAnimation);
                }
                usernameScoreText.color = Color.white;
            }
            MonoBehaviourAttacher.AttachTextHoverEffect(usernameScoreText.gameObject, false, "", "", FontStyles.Underline, FontStyles.Normal);
            UnityMainThreadTaskScheduler.Factory.StartNew(() =>
            {
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
