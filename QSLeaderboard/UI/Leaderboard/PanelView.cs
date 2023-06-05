using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace QSLeaderboard.UI.Leaderboard
{
    [HotReload(RelativePathToLayout = @"./BSML/PanelView.bsml")]
    [ViewDefinition("QSLeaderboard.UI.Leaderboard.BSML.PanelView.bsml")]
    internal class PanelView : BSMLAutomaticViewController
    {
    }
}
