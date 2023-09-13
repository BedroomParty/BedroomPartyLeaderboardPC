using BedroomPartyLeaderboard.Utils;
using HarmonyLib;
using System;

namespace BedroomPartyLeaderboard.HarmonyPatches
{
    internal class ExtraSongData
    {
        public static int currentPerfectHits = 0;
        public static int highestPerfectStreak = 0;

        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Init")]
        internal class StandardLevelScenesTransitionSetupDataSOInit
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
                }
                else if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorB)
                {
                    ExtraSongDataHolder.avgHandAccLeft.Add(____cutScoreBuffer.cutScore);
                }

                if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorA)
                {
                    ExtraSongDataHolder.avgHandTDLeft.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));
                }
                else if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorB)
                {
                    ExtraSongDataHolder.avgHandTDRight.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));
                }


                if (____cutScoreBuffer.cutScore == 115)
                {
                    currentPerfectHits++;
                    if (currentPerfectHits > highestPerfectStreak)
                    {
                        highestPerfectStreak = currentPerfectHits;
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
