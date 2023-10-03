using System.Collections.Generic;

namespace BedroomPartyLeaderboard.Utils
{
    internal static class ExtraSongDataHolder
    {
        internal static int pauses;
        internal static List<int> rightHandAccuracy = new();
        internal static List<int> leftHandAccuracy = new();
        internal static List<float> rightHandTimeDependency = new();
        internal static List<float> leftHandTimeDependency = new();

        internal static int perfectStreak = 0;

        internal static void reset()
        {
            pauses = 0;
            rightHandAccuracy.Clear();
            leftHandAccuracy.Clear();
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

        internal static float GetFcAcc()
        {
            int blocksHit = leftHandAccuracy.Count + rightHandAccuracy.Count;
            if (blocksHit == 0) return 0.0f;
            float averagehitscore = ((float)GetTotalFromList(leftHandAccuracy) + (float)GetTotalFromList(rightHandAccuracy)) / (float)blocksHit;
            float fcAcc = averagehitscore / 115 * 100;
            return fcAcc;
        }

    }
}
