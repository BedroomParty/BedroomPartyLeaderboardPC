using System.Collections.Generic;

namespace BedroomPartyLeaderboard.Utils
{
    internal static class ExtraSongDataHolder
    {
        internal static int pauses;
        internal static List<float> avgHandAccRight;
        internal static List<float> avgHandAccLeft;
        internal static List<float> avgHandTDRight;
        internal static List<float> avgHandTDLeft;
        internal static int perfectStreak;

        internal static void reset()
        {
            pauses = 0;
            avgHandAccRight.Clear();
            avgHandAccLeft.Clear();
            avgHandTDRight.Clear();
            avgHandTDLeft.Clear();
            perfectStreak = 0;
        }

        internal static float GetAverageFromList(List<float> list)
        {
            float sum = 0;
            foreach (float f in list)
            {
                sum += f;
            }
            return sum / list.Count;
        }
    }
}
