using ModestTree;
using SiraUtil.Logging;
using System;
using System.Text.RegularExpressions;
using UnityEngine.XR;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class HeadsetUtils
    {
        [Inject] internal SiraLog _log;

        internal string hmdName;
        internal HMD hmd;

        private HMD HMDFromName(string hmdName)
        {
            var lowerHmd = hmdName.ToLower();

            if (lowerHmd.Contains("pico") && lowerHmd.Contains("4")) return HMD.picoNeo4;
            if (lowerHmd.Contains("pico neo") && lowerHmd.Contains("3")) return HMD.picoNeo3;
            if (lowerHmd.Contains("pico neo") && lowerHmd.Contains("2")) return HMD.picoNeo2;
            if (lowerHmd.Contains("vive pro 2")) return HMD.vivePro2;
            if (lowerHmd.Contains("vive elite")) return HMD.viveElite;
            if (lowerHmd.Contains("focus3")) return HMD.viveFocus;
            if (lowerHmd.Contains("miramar")) return HMD.miramar;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("8k")) return HMD.pimax8k;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("5k")) return HMD.pimax5k;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("artisan")) return HMD.pimaxArtisan;
            if (lowerHmd.Contains("pimax") && lowerHmd.Contains("crystal")) return HMD.pimaxCrystal;

            if (lowerHmd.Contains("controllable")) return HMD.controllable;

            if (lowerHmd.Contains("hp reverb")) return HMD.hpReverb;
            if (lowerHmd.Contains("samsung windows")) return HMD.samsungWmr;
            if (lowerHmd.Contains("qiyu dream")) return HMD.qiyuDream;
            if (lowerHmd.Contains("disco")) return HMD.disco;
            if (lowerHmd.Contains("lenovo explorer")) return HMD.lenovoExplorer;
            if (lowerHmd.Contains("acer")) return HMD.acerWmr;
            if (lowerHmd.Contains("arpara")) return HMD.arpara;
            if (lowerHmd.Contains("dell visor")) return HMD.dellVisor;

            if (lowerHmd.Contains("e3")) return HMD.e3;
            if (lowerHmd.Contains("e4")) return HMD.e4;

            if (lowerHmd.Contains("vive dvt")) return HMD.viveDvt;
            if (lowerHmd.Contains("3glasses s20")) return HMD.glasses20;
            if (lowerHmd.Contains("hedy")) return HMD.hedy;
            if (lowerHmd.Contains("vaporeon")) return HMD.vaporeon;
            if (lowerHmd.Contains("huaweivr")) return HMD.huaweivr;
            if (lowerHmd.Contains("asus mr0")) return HMD.asusWmr;
            if (lowerHmd.Contains("cloudxr")) return HMD.cloudxr;
            if (lowerHmd.Contains("vridge")) return HMD.vridge;
            if (lowerHmd.Contains("medion mixed reality")) return HMD.medion;

            if (lowerHmd.Contains("quest") && lowerHmd.Contains("2")) return HMD.quest2;
            if (lowerHmd.Contains("quest") && lowerHmd.Contains("3")) return HMD.quest3;
            if (lowerHmd.Contains("quest") && lowerHmd.Contains("pro")) return HMD.questPro;

            if (lowerHmd.Contains("vive cosmos")) return HMD.viveCosmos;
            if (lowerHmd.Contains("vive_cosmos")) return HMD.viveCosmos;
            if (lowerHmd.Contains("index")) return HMD.index;
            if (lowerHmd.Contains("quest")) return HMD.quest;
            if (lowerHmd.Contains("rift s")) return HMD.riftS;
            if (lowerHmd.Contains("rift_s")) return HMD.riftS;
            if (lowerHmd.Contains("windows")) return HMD.wmr;
            if (lowerHmd.Contains("vive pro")) return HMD.vivePro;
            if (lowerHmd.Contains("vive_pro")) return HMD.vivePro;
            if (lowerHmd.Contains("vive")) return HMD.vive;
            if (lowerHmd.Contains("rift")) return HMD.rift;

            return HMD.unknown;
        }
        internal enum HMD
        {
            unknown = 0,
            rift = 1,
            riftS = 16,
            quest = 32,
            quest2 = 256,
            quest3 = 512,
            vive = 2,
            vivePro = 4,
            viveCosmos = 128,
            wmr = 8,

            picoNeo3 = 33,
            picoNeo2 = 34,
            vivePro2 = 35,
            viveElite = 36,
            miramar = 37,
            pimax8k = 38,
            pimax5k = 39,
            pimaxArtisan = 40,
            hpReverb = 41,
            samsungWmr = 42,
            qiyuDream = 43,
            disco = 44,
            lenovoExplorer = 45,
            acerWmr = 46,
            viveFocus = 47,
            arpara = 48,
            dellVisor = 49,
            e3 = 50,
            viveDvt = 51,
            glasses20 = 52,
            hedy = 53,
            vaporeon = 54,
            huaweivr = 55,
            asusWmr = 56,
            cloudxr = 57,
            vridge = 58,
            medion = 59,
            picoNeo4 = 60,
            questPro = 61,
            pimaxCrystal = 62,
            e4 = 63,
            index = 64,
            controllable = 65,
        }

        internal string GetHMDStringFromInt(int value)
        {
            if (Enum.IsDefined(typeof(HMD), value))
            {
                HMD hmd = (HMD)value;
                string hmdString = Regex.Replace(hmd.ToString(), "(\\B[A-Z])", " $1");
                return hmdString;
            }
            else
            {
                return "Unknown HMD";
            }
        }

        internal string GetIntFromHMDString(string value)
        {
            if (Enum.IsDefined(typeof(HMD), value))
            {
                HMD hmd = (HMD)Enum.Parse(typeof(HMD), value, true);
                return ((int)hmd).ToString();
            }
            else
            {
                return "0";
            }
        }

        internal void GetHMDInfo(out string hmdName, out HMD hmd)
        {
            hmdName = GetDeviceHMD();
            hmd = HMDFromName(hmdName);
            _log.Notice($"HMD Name: {hmdName}");
            _log.Notice($"HMD: {hmd}");
        }

        internal string GetDeviceHMD()
        {
            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var hmd = $"{XRDevice.model}";
#pragma warning restore CS0618 // Type or member is obsolete
                if (hmd == null || hmd.IsEmpty() || hmd == "(xrdevice):")
                {
                    hmd = "unknown";
                }
                return GetHMDStringFromInt((int)HMDFromName(hmd));
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"Exception getting HMD: {e}");
                return "unknown";
            }
        }
    }
}