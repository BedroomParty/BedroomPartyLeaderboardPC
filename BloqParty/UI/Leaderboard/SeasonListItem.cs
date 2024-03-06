using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.Linq;
using UnityEngine;
using Application = UnityEngine.Application;

namespace BloqParty.UI.Leaderboard
{
    internal class SeasonListItem
    {
        public int index;

        [UIValue("seasonNumber")] public string seasonNumber;
        [UIValue("seasonDescription")] public string seasonDescription;

        [UIValue("rank")] public string rankText;
        [UIValue("pp")] public string ppText;


        [UIComponent("seasonImage")] public ImageView seasonImage;
        private Sprite seasonImageSprite;


        public SeasonListItem(int index, string seasonNumber, string seasonDescription, Sprite seasonImageSprite, string Rank, string PP)
        {
            this.index = index;
            this.seasonNumber = seasonNumber;
            this.seasonDescription = seasonDescription;
            this.seasonImageSprite = seasonImageSprite;
            this.rankText = Rank;
            this.ppText = PP;
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            seasonImage.sprite = seasonImageSprite;
            this.seasonImage.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UINoGlowRoundEdge");
        }


        [UIAction("seasonPlaylistClicked")]
        public void seasonPlaylistClicked()
        {
            Application.OpenURL("https://thebedroom.party/playlist/" + index);
        }

        [UIAction("seasonLeaderboardClicked")]
        public void seasonLeaderboardClicked()
        {
            Application.OpenURL("https://thebedroom.party/season/" + index);
        }
    }
}
