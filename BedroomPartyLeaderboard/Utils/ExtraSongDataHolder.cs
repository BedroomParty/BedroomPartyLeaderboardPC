using System.Collections.Generic;

namespace BedroomPartyLeaderboard.Utils
{
    internal static class ExtraSongDataHolder
    {
        internal static int pauses;
        internal static List<int> avgHandAccRight = new();
        internal static List<int> avgHandAccLeft = new();
        internal static List<float> avgHandTDRight = new();
        internal static List<float> avgHandTDLeft = new();

        internal static int perfectStreak = 0;

        internal static void reset()
        {
            pauses = 0;
            avgHandAccRight.Clear();
            avgHandAccLeft.Clear();
            avgHandTDRight.Clear();
            avgHandTDLeft.Clear();
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
            int blocksHit = avgHandAccLeft.Count + avgHandAccRight.Count;
            if (blocksHit == 0) return 0.0f;
            float averagehitscore = ((float)GetTotalFromList(avgHandAccLeft) + (float)GetTotalFromList(avgHandAccRight)) / (float)blocksHit;
            float fcAcc = averagehitscore / 115 * 100;
            return fcAcc;
        }

    }
}
