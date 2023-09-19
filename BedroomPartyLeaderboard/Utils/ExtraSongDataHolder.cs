using System.Collections.Generic;

namespace BedroomPartyLeaderboard.Utils
{
    internal static class ExtraSongDataHolder
    {
        internal static int pauses;
        internal static List<float> avgHandAccRight = new();
        internal static List<float> avgHandAccLeft = new();
        internal static List<float> avgHandTDRight = new();
        internal static List<float> avgHandTDLeft = new();
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
            if(list.Count == 0)
            {
                return 0;
            }
            return sum / list.Count;
            // it sure does
        }
    }
}
