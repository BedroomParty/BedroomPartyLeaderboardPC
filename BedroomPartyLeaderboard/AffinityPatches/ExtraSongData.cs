using BedroomPartyLeaderboard.Utils;
using SiraUtil.Affinity;
using System;

namespace BedroomPartyLeaderboard.AffinityPatches
{
    internal class ExtraSongData : IAffinity
    {
        public static int currentPerfectHits = 0;
        public static int highestPerfectStreak = 0;

        [AffinityPostfix]
        [AffinityPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Init")]
        public static void Postfix()
        {
            ExtraSongDataHolder.reset();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteController), "SendNoteWasCutEvent")]
        public static void Postfix2(ref NoteCutInfo __noteCutInfo)
        {
            if (__noteCutInfo.noteData.colorType == ColorType.ColorA)
            {
                ExtraSongDataHolder.avgHandTDLeft.Add(Math.Abs(__noteCutInfo.cutNormal.z));
            }
            else if (__noteCutInfo.noteData.colorType == ColorType.ColorB)
            {
                ExtraSongDataHolder.avgHandTDRight.Add(Math.Abs(__noteCutInfo.cutNormal.z));
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(FlyingScoreEffect), "HandleCutScoreBufferDidFinish")]
        public static void Postfix3(ref CutScoreBuffer __cutScoreBuffer)
        {
            if (__cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorA)
            {
                ExtraSongDataHolder.avgHandAccLeft.Add(__cutScoreBuffer.cutScore);
            }
            else if (__cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorB)
            {
                ExtraSongDataHolder.avgHandAccLeft.Add(__cutScoreBuffer.cutScore);
            }

            if (__cutScoreBuffer.cutScore == 115)
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

        [AffinityPostfix]
        [AffinityPatch(typeof(PauseController), "Pause")]
        public static void Postfix4()
        {
            ExtraSongDataHolder.pauses++;
        }
    }
}
