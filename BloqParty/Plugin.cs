﻿using BloqParty.Installers;
using IPA;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BloqParty
{
    [Plugin(RuntimeOptions.SingleStartInit), NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

#if DEBUG
        internal const bool isDev = true;
#else
        internal const bool isDev = false;
#endif

        [Init]
        public Plugin(Zenjector zenjector)
        {
            zenjector.UseLogger(Log);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.GameCore);
        }
    }
}
