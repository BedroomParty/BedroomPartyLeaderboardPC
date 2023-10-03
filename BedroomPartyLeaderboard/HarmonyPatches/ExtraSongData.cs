using BedroomPartyLeaderboard.Utils;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using Zenject;

namespace BedroomPartyLeaderboard.AffinityPatches
{
    internal class ExtraSongData : IAffinity
    {
        [Inject] private readonly SiraLog _log;
        internal int currentPerfectHits = 0;
        internal int highestPerfectStreak = 0;

        [AffinityPostfix]
        [AffinityPatch(typeof(AudioTimeSyncController), "Start")]
        public void Postfixaaa()
        {
            _log.Info("Resetting ExtraSongData");
            ExtraSongDataHolder.reset();
            currentPerfectHits = 0;
            highestPerfectStreak = 0;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(PauseMenuManager), "RestartButtonPressed")]
        public void Postfixsdasda()
        {
            _log.Info("Resetting ExtraSongData");
            ExtraSongDataHolder.reset();
            currentPerfectHits = 0;
            highestPerfectStreak = 0;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidFinish))]
        public void Postfixfgasg(ref CutScoreBuffer ____cutScoreBuffer)
        {
            if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorA)
            {
                ExtraSongDataHolder.leftHandAccuracy.Add(____cutScoreBuffer.cutScore);
                ExtraSongDataHolder.leftHandTimeDependency.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));

            }
            else if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorB)
            {
                ExtraSongDataHolder.rightHandAccuracy.Add(____cutScoreBuffer.cutScore);
                ExtraSongDataHolder.rightHandTimeDependency.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));
            }

            if (____cutScoreBuffer.cutScore == 115)
            {
                currentPerfectHits++;
                if (currentPerfectHits > highestPerfectStreak)
                {
                    highestPerfectStreak = currentPerfectHits;
                    ExtraSongDataHolder.perfectStreak = highestPerfectStreak;
                }
            }
            else
            {
                currentPerfectHits = 0;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GamePause), nameof(GamePause.Pause))]
        public void Postfixasgasg()
        {
            ExtraSongDataHolder.pauses++;
        }
    }
}
