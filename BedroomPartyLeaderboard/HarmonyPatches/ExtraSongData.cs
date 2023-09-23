using BedroomPartyLeaderboard.Utils;
using HarmonyLib;
using System;

namespace BedroomPartyLeaderboard.HarmonyPatches
{
    internal class ExtraSongData
    {
        public static int currentPerfectHits = 0;
        public static int highestPerfectStreak = 0;

        [HarmonyPatch(typeof(AudioTimeSyncController), "Start")]
        internal class AudioTimeSyncControllerStart
        {
            private static void Postfix()
            {
                ExtraSongDataHolder.reset();
                currentPerfectHits = 0;
                highestPerfectStreak = 0;
            }
        }

        [HarmonyPatch(typeof(PauseMenuManager), "RestartButtonPressed")]
        internal class PauseMenuManagerRestartButtonPressed
        {
            private static void Postfix()
            {
                ExtraSongDataHolder.reset();
                currentPerfectHits = 0;
                highestPerfectStreak = 0;
            }
        }


        [HarmonyPatch(typeof(FlyingScoreEffect), "HandleCutScoreBufferDidFinish")]
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

        [HarmonyPatch(typeof(PauseController), "Pause")]
        internal class PauseControllerPause
        {
            public static void Postfix()
            {
                ExtraSongDataHolder.pauses++;
            }
        }
    }
}
