using BedroomPartyLeaderboard.UI.Leaderboard;
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

        internal static ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardEntry entry, int score, int rankFUCK)
        {
            try
            {
                string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
                string formattedCombo = (bool)entry.fullCombo
                    ? " -<color=green> FC </color>"
                    : string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);
                string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

                string result;
                if (entry.userID == "76561199077754911")
                {
                    entry.userName = $"<color=#6488ea>{entry.userName}</color>";
                }

                result = "<size=90%>" + entry.userName.TrimEnd() + formattedAcc + formattedCombo + formattedMods + "</size>";
                entry.rank = rankFUCK;
                return new ScoreData(score, result, rankFUCK, false);
            }
            catch
            {
                return new ScoreData(0, "<color=red>Error</color>", rankFUCK, false);
            }
        }
    }
}
