using BedroomPartyLeaderboard.UI.Leaderboard;
using System;
using System.Collections.Generic;
using static BedroomPartyLeaderboard.Utils.UIUtils;
using static LeaderboardTableView;

namespace BedroomPartyLeaderboard.Utils
{
    internal static class LeaderboardDataUtils
    {
        internal static List<ScoreData> CreateLeaderboardData(List<LeaderboardData.LeaderboardEntry> leaderboard, int page, List<ButtonHolder> buttonHolders)
        {
            List<ScoreData> tableData = new();
            for (int i = 0; i < leaderboard.Count; i++)
            {
                int score = (int)leaderboard[i].modifiedScore;
                int rank = (((page + 1) * 10) - (10 - i)) + 1;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], (int)score, (int)rank));
                LeaderboardView.buttonEntryArray[i] = leaderboard[i];
                buttonHolders[i].infoButton.gameObject.SetActive(false);
            }
            return tableData;
        }

        internal static string GetFormattedScore(int score)
        {
            string result = "";
            if (score >= 1000000)
            {
                result = $"{score / 1000000}M";
            }
            else if (score >= 1000)
            {
                result = $"{score / 1000}K";
            }
            else
            {
                result = $"{score}";
            }
            return result;
        }

        internal static string GetAccPercentFromHand(float handAcc)
        {
            return GetAccPercentFromHandFloat(handAcc).ToString("0.00") + "%";
        }

        internal static float GetAccPercentFromHandFloat(float handAcc)
        {
            return (handAcc / 115) * 100;
        }

        internal static int GetUserScorePos(List<LeaderboardData.LeaderboardEntry> leaderboard, string userID)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (leaderboard[i].userID == userID)
                {
                    return i;
                }
            }
            return -1;
        }

        internal static string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();
            System.Array.Reverse(charArray);
            return new string(charArray);
        }

        internal static ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardEntry entry, int score, int rankFUCK)
        {
            try
            {
                string formattedAcc = $" - (<color=#FF69B4>{entry.acc:0.00}%</color>)";
                string formattedCombo = (bool)entry.fullCombo
                    ? $" - <color={Constants.goodToast}> FC </color>"
                    : $" - <color={Constants.badToast}>x{entry.badCutCount + entry.missCount} </color>";
                string formattedMods = $"  <size=60%>{entry.mods}</size>";

                string result;
                if (entry.userID == "76561199077754911")
                {
                    entry.userName = $"<color=#6488ea><rotate=180>{ReverseString(entry.userName)}</rotate></color>";
                }

                result = "<size=90%>" + entry.userName.TrimEnd() + formattedAcc + formattedCombo + formattedMods + "</size>";
                entry.rank = rankFUCK;
                return new ScoreData(score, result, rankFUCK, false);
            }
            catch
            {
                return new ScoreData(0, $"<color={Constants.badToast}>Error</color>", rankFUCK, false);
            }
        }
    }
}
