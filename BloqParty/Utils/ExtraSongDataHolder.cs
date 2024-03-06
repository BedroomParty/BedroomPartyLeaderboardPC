using ModestTree;
using System;
using System.Collections.Generic;

namespace BloqParty.Utils
{
    internal static class ExtraSongDataHolder
    {
        internal static int pauses;
        internal static List<int> rightHandAverageScore = new();
        internal static List<int> leftHandAverageScore = new();
        internal static List<float> rightHandTimeDependency = new();
        internal static List<float> leftHandTimeDependency = new();
        internal static List<Tuple<int, int>> totalBlocksHit = new();
        internal static int perfectStreak = 0;

        internal static void reset()
        {
            pauses = 0;
            rightHandAverageScore.Clear();
            leftHandAverageScore.Clear();
            rightHandTimeDependency.Clear();
            leftHandTimeDependency.Clear();
            perfectStreak = 0;
        }

        internal static float GetAverageFromList(List<int> list)
        {
            float sum = 0;
            foreach (float f in list)
            {
                sum += f;
            }
            if (list.Count == 0)
            {
                return 0;
            }
            return sum / list.Count;
        }

        internal static float GetAverageFromList(List<float> list)
        {
            float sum = 0;
            foreach (float f in list)
            {
                sum += f;
            }
            if (list.Count == 0)
            {
                return 0;
            }
            return sum / list.Count;
        }

        internal static int GetTotalFromList(List<int> list)
        {
            int sum = 0;
            foreach (int i in list)
            {
                sum += i;
            }
            return sum;
        }


        internal static int GetMaxScoreForScoringType(int scoringType)
        {
            return scoringType switch
            {
                1 or 2 or 3 => 115,
                4 => 70,
                5 => 20,
                _ => 0,
            };
        }

        internal static float GetFcAcc(float multiplier)
        {
            if (totalBlocksHit.IsEmpty()) return 0.0f;
            float realScore = 0, maxScore = 0;
            foreach (var p in totalBlocksHit)
            {
                realScore += p.Item1 * multiplier;
                maxScore += GetMaxScoreForScoringType(p.Item2);
            }
            float fcAcc = (float)realScore / (float)maxScore * 100.0f;
            return fcAcc;
        }
    }
}
