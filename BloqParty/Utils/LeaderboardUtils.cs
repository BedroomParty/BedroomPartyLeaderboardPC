using BloqParty.UI.Leaderboard;
using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using System;

namespace BloqParty.Utils
{
    internal class LeaderboardUtils : CustomLeaderboard, IDisposable
    {
        protected override ViewController panelViewController { get; }
        protected override ViewController leaderboardViewController { get; }
        private readonly CustomLeaderboardManager _manager;
        public LeaderboardUtils(CustomLeaderboardManager manager, PanelView panelView, LeaderboardView leaderboardView)
        {
            _manager = manager;
            panelViewController = panelView;
            leaderboardViewController = leaderboardView;

            _manager.Register(this);
        }
        public void Dispose()
        {
            _manager.Unregister(this);
        }
    }
}
