using BloqParty.UI.Leaderboard;
using BloqParty.Utils;
using IPA.Utilities;
using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace BloqParty.AffinityPatches
{
    internal class Results : IAffinity
    {
#pragma warning disable IDE0060
#pragma warning disable IDE1006 // Naming Styles
        [Inject] private readonly RequestUtils _requestUtils;
        [Inject] private readonly AuthenticationManager _authenticationManager;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly UIUtils _uiUtils;
        [Inject] private readonly PlayerUtils _playerUtils;
        [Inject] private readonly SiraLog _log;

        float GetModifierScoreMultiplier(LevelCompletionResults results, GameplayModifiersModelSO modifiersModel)
        {
            return modifiersModel.GetTotalMultiplier(modifiersModel.CreateModifierParamsList(results.gameplayModifiers), results.energy);
        }

        public static string GetModifiersString(LevelCompletionResults levelCompletionResults)
        {
            string mods = "";
            GameplayModifiers gameplayModifiers = levelCompletionResults.gameplayModifiers;
            if (gameplayModifiers.noFailOn0Energy && levelCompletionResults.energy == 0) mods += "NF";
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster) mods += "FS ";
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast) mods += "SF ";
            if (gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery) mods += "BE ";
            if (gameplayModifiers.proMode) mods += "PM ";
            if (gameplayModifiers.instaFail) mods += "IF ";
            if (gameplayModifiers.failOnSaberClash) mods += "SC ";
            if (gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles) mods += "NO ";
            if (gameplayModifiers.noBombs) mods += "NB ";
            if (gameplayModifiers.strictAngles) mods += "SA ";
            if (gameplayModifiers.disappearingArrows) mods += "DA ";
            if (gameplayModifiers.ghostNotes) mods += "GN ";
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower) mods += "SS ";
            if (gameplayModifiers.smallCubes) mods += "SC ";
            if (gameplayModifiers.noArrows) mods += "NA ";
            return mods.TrimEnd();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
        private void Postfix(ref PlayerData playerData, ref PlayerLevelStatsData playerLevelStats, ref LevelCompletionResults levelCompletionResults, ref IReadonlyBeatmapData transformedBeatmapData, ref IDifficultyBeatmap difficultyBeatmap, ref PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            _log.Info("Begin Score Postfix");
            if (BS_Utils.Gameplay.ScoreSubmission.Disabled) return;
            if (!difficultyBeatmap.level.levelID.Contains("custom")) return;
            if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) return;
            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformedBeatmapData);
            int modifiedScore = levelCompletionResults.modifiedScore;
            int multipliedScore = levelCompletionResults.multipliedScore;
            if (modifiedScore == 0 || maxScore == 0) return;
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed) return;
            float acc = modifiedScore / maxScore * 100;
            int badCut = levelCompletionResults.badCutsCount;
            int misses = levelCompletionResults.missedCount;
            bool fc = levelCompletionResults.fullCombo;
            string mapId = difficultyBeatmap.level.levelID.Substring(13).Split('_')[0];
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = playerLevelStats.beatmapCharacteristic.serializedName;
            (string, int, string) balls = (mapId, difficulty, mapType);
            string mods = GetModifiersString(levelCompletionResults);

            int pauses = ExtraSongDataHolder.pauses;
            int maxCombo = levelCompletionResults.maxCombo;
            float rightHandAverageScore = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.rightHandAverageScore);
            float leftHandAverageScore = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.leftHandAverageScore);
            int perfectStreak = ExtraSongDataHolder.perfectStreak;

            float rightHandTimeDependency = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.rightHandTimeDependency);
            float leftHandTimeDependency = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.leftHandTimeDependency);
            float fcAcc;
            if (fc) fcAcc = acc;
            else fcAcc = ExtraSongDataHolder.GetFcAcc(GetModifierScoreMultiplier(levelCompletionResults, platformLeaderboardsModel.GetField<GameplayModifiersModelSO, PlatformLeaderboardsModel>("_gameplayModifiersModel")));

            UnityMainThreadTaskScheduler.Factory.StartNew(() => _requestUtils.HandleLBUpload());
            string json = getLBUploadJSON(balls, _authenticationManager._localPlayerInfo.userID, badCut, misses, fc, acc, mods, multipliedScore, modifiedScore, pauses, maxCombo, rightHandAverageScore, leftHandAverageScore, perfectStreak, rightHandTimeDependency, leftHandTimeDependency, fcAcc);
            _requestUtils.SetBeatMapData(mapId, json);
            _log.Info("End Score Postfix");
        }

        private string getLBUploadJSON((string, int, string) balls, string userID, int badCuts, int misses, bool fullCOmbo, float acc, string mods, int multipliedScore, int modifiedScore, int pauses, int maxCombo, float avgAccRight, float avgAccLeft, int perfectStreak, float rightHandTimeDependency, float leftHandTimeDependency, float fcAcc)
        {
            JObject Data = new()
            {
                { "difficulty", balls.Item2 },
                { "characteristic", balls.Item3 },
                { "id", userID },
                { "badCuts", badCuts },
                { "misses", misses },
                { "fullCombo", fullCOmbo },
                { "accuracy", acc },
                { "modifiedScore", modifiedScore },
                { "multipliedScore", multipliedScore },
                { "modifiers", mods },
                { "pauses", pauses },
                { "maxCombo", maxCombo },
                { "rightHandTimeDependency", rightHandTimeDependency },
                { "leftHandTimeDependency", leftHandTimeDependency },
                { "rightHandAverageScore", avgAccRight },
                { "leftHandAverageScore", avgAccLeft},
                { "perfectStreak", perfectStreak },
                { "fcAccuracy", fcAcc }
            };
            return Data.ToString();
        }
    }
}
