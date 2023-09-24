using BedroomPartyLeaderboard.Utils;
using HarmonyLib;
using SiraUtil.Affinity;
using System;

namespace BedroomPartyLeaderboard.AffinityPatches
{
    internal class ExtraSongData : IAffinity
    {
        internal static int currentPerfectHits = 0;
        internal static int highestPerfectStreak = 0;

        [AffinityPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Start))]
        internal class AudioTimeSyncControllerStart
        {
            private static void Postfix()
            {
                ExtraSongDataHolder.reset();
                currentPerfectHits = 0;
                highestPerfectStreak = 0;
            }
        }

        [AffinityPatch(typeof(PauseMenuManager), nameof(PauseMenuManager.RestartButtonPressed))]
        internal class PauseMenuManagerRestartButtonPressed
        {
            private static void Postfix()
            {
                ExtraSongDataHolder.reset();
                currentPerfectHits = 0;
                highestPerfectStreak = 0;
            }
        }


        [AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidFinish))]
        internal class FlyingScoreEffectHandleCutScoreBufferDidFinish
        {
            public static void Postfix(ref CutScoreBuffer ____cutScoreBuffer)
            {
                if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorA)
                {
                    ExtraSongDataHolder.avgHandAccLeft.Add(____cutScoreBuffer.cutScore);
                    ExtraSongDataHolder.avgHandTDLeft.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));

                }
                else if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorB)
                {
                    ExtraSongDataHolder.avgHandAccRight.Add(____cutScoreBuffer.cutScore);
                    ExtraSongDataHolder.avgHandTDRight.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));
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
        }

        [AffinityPatch(typeof(PauseController), nameof(PauseController.Pause))]
        internal class PauseControllerPause
        {
            public static void Postfix()
            {
                ExtraSongDataHolder.pauses++;
            }
        }
    }
}
