using BedroomPartyLeaderboard.UI.Leaderboard;
using BedroomPartyLeaderboard.Utils;
using IPA.Utilities.Async;
using Newtonsoft.Json.Linq;
using SiraUtil.Affinity;
using Zenject;

namespace BedroomPartyLeaderboard.AffinityPatches
{
    internal class Results : IAffinity
    {
        [Inject] private readonly RequestUtils _requestUtils;
        [Inject] private readonly AuthenticationManager _authenticationManager;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly UIUtils _uiUtils;
        [Inject] private readonly PlayerUtils _playerUtils;

        public static string GetModifiersString(LevelCompletionResults levelCompletionResults)
        {
            string mods = "";

            if (levelCompletionResults.gameplayModifiers.noFailOn0Energy && levelCompletionResults.energy == 0)
            {
                mods += "NF";
            }

            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster)
            {
                mods += "FS ";
            }

            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast)
            {
                mods += "SF ";
            }

            if (levelCompletionResults.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery)
            {
                mods += "BE ";
            }

            if (levelCompletionResults.gameplayModifiers.proMode)
            {
                mods += "PM ";
            }

            if (levelCompletionResults.gameplayModifiers.instaFail)
            {
                mods += "IF ";
            }

            if (levelCompletionResults.gameplayModifiers.failOnSaberClash)
            {
                mods += "SC ";
            }

            if (levelCompletionResults.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles)
            {
                mods += "NO ";
            }

            if (levelCompletionResults.gameplayModifiers.noBombs)
            {
                mods += "NB ";
            }

            if (levelCompletionResults.gameplayModifiers.strictAngles)
            {
                mods += "SA ";
            }

            if (levelCompletionResults.gameplayModifiers.disappearingArrows)
            {
                mods += "DA ";
            }

            if (levelCompletionResults.gameplayModifiers.ghostNotes)
            {
                mods += "GN ";
            }

            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower)
            {
                mods += "SS ";
            }

            if (levelCompletionResults.gameplayModifiers.smallCubes)
            {
                mods += "SC ";
            }

            if (levelCompletionResults.gameplayModifiers.noArrows)
            {
                mods += "NA ";
            }

            return mods.TrimEnd();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
        private void Postfix(ref PlayerData playerData, ref PlayerLevelStatsData playerLevelStats, ref LevelCompletionResults levelCompletionResults, ref IReadonlyBeatmapData transformedBeatmapData, ref IDifficultyBeatmap difficultyBeatmap, ref PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            if (BS_Utils.Gameplay.ScoreSubmission.Disabled) return;
            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformedBeatmapData);
            int modifiedScore = levelCompletionResults.modifiedScore;
            int multipliedScore = levelCompletionResults.multipliedScore;
            if (modifiedScore == 0 || maxScore == 0) return;
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed) return;
            float acc = modifiedScore / maxScore * 100;
            int score = levelCompletionResults.modifiedScore;
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
            float avgHandAccRight = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.avgHandAccRight);
            float avgHandAccLeft = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.avgHandAccLeft);
            int perfectStreak = ExtraSongDataHolder.perfectStreak;

            float avgHandTDRight = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.avgHandTDRight);
            float avgHandTDLeft = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.avgHandTDLeft);

            UnityMainThreadTaskScheduler.Factory.StartNew(() => _requestUtils.HandleLBUpload());
            string json = getLBUploadJSON(balls, _authenticationManager._localPlayerInfo.userID, badCut, misses, fc, acc, mods, multipliedScore, modifiedScore, pauses, maxCombo, avgHandAccRight, avgHandAccLeft, perfectStreak, avgHandTDRight, avgHandTDLeft);
            _requestUtils.SetBeatMapData(json, result =>
            {
            });
        }

        private string getLBUploadJSON((string, int, string) balls, string userID, int badCuts, int misses, bool fullCOmbo, float acc, string mods, int multipliedScore, int modifiedScore, int pauses, int maxCombo, float avgAccRight, float avgAccLeft, int perfectStreak, float avgHandTDRight, float avgHandTDLeft)
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
                { "avgHandTDRight", avgHandTDLeft },
                { "avgHandTDLeft", avgHandTDRight },
                { "avgHandAccRight", avgAccRight },
                { "avgHandAccLeft", avgAccLeft},
                { "perfectStreak", perfectStreak }
            };
            return Data.ToString();
        }
    }
}
