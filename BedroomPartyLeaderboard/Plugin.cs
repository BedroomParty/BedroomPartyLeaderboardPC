﻿using BedroomPartyLeaderboard.Installers;
using HarmonyLib;
using IPA;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace BedroomPartyLeaderboard
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(Zenjector zenjector)
        {
            zenjector.UseLogger(Log);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.GameCore);
        }
    }
}
