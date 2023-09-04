using BedroomPartyLeaderboard.Utils;
using SiraUtil.Affinity;
using Zenject;

namespace BedroomPartyLeaderboard.AffinityPatches
{
    internal class Results : IAffinity
    {
        [Inject] readonly RequestUtils _requestUtils;
        [Inject] private readonly PlayerUtils _playerUtils;
        public static string GetModifiersString(LevelCompletionResults levelCompletionResults)
        {
            string mods = "";

            if (levelCompletionResults.gameplayModifiers.noFailOn0Energy && levelCompletionResults.energy == 0) mods += "NF";
            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster) mods += "FS ";
            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast) mods += "SF ";
            if (levelCompletionResults.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery) mods += "BE ";
            if (levelCompletionResults.gameplayModifiers.proMode) mods += "PM ";
            if (levelCompletionResults.gameplayModifiers.instaFail) mods += "IF ";
            if (levelCompletionResults.gameplayModifiers.failOnSaberClash) mods += "SC ";
            if (levelCompletionResults.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles) mods += "NO ";
            if (levelCompletionResults.gameplayModifiers.noBombs) mods += "NB ";
            if (levelCompletionResults.gameplayModifiers.strictAngles) mods += "SA ";
            if (levelCompletionResults.gameplayModifiers.disappearingArrows) mods += "DA ";
            if (levelCompletionResults.gameplayModifiers.ghostNotes) mods += "GN ";
            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower) mods += "SS ";
            if (levelCompletionResults.gameplayModifiers.smallCubes) mods += "SC ";
            if (levelCompletionResults.gameplayModifiers.noArrows) mods += "NA ";
            return mods.TrimEnd();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
        private void Postfix(ref PlayerData playerData, ref PlayerLevelStatsData playerLevelStats, ref LevelCompletionResults levelCompletionResults, ref IReadonlyBeatmapData transformedBeatmapData, ref IDifficultyBeatmap difficultyBeatmap, ref PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            if (!_playerUtils.IsAuthed) return;
            if (BS_Utils.Gameplay.ScoreSubmission.Disabled) return;
            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformedBeatmapData);
            int modifiedScore = levelCompletionResults.modifiedScore;
            int multipliedScore = levelCompletionResults.multipliedScore;
            if (modifiedScore == 0 || maxScore == 0) return;
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed) return;

            float acc = (modifiedScore / maxScore) * 100;
            int score = levelCompletionResults.modifiedScore;
            int badCut = levelCompletionResults.badCutsCount;
            int misses = levelCompletionResults.missedCount;
            bool fc = levelCompletionResults.fullCombo;

            string mapId = difficultyBeatmap.level.levelID.Substring(13);

            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = playerLevelStats.beatmapCharacteristic.serializedName;

            (string, int, string) balls = (mapId, difficulty, mapType);

            string mods = GetModifiersString(levelCompletionResults);


            _requestUtils.SetBeatMapData(balls, _playerUtils.localPlayerInfo.authKey, _playerUtils.localPlayerInfo.username, badCut, misses, fc, acc, score, mods, multipliedScore, modifiedScore, result =>
            {
                Plugin.Log.Info("_requestUtils.SetBeatMapData");
            });
        }
    }
}
