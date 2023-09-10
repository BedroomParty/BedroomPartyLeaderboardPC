using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;
using Zenject;
using Application = UnityEngine.Application;

namespace BedroomPartyLeaderboard.UI.Leaderboard
{
    internal class SeasonListItem
    {
        public int index;

        [UIValue("seasonNumber")] public string seasonNumber;
        [UIValue("seasonDescription")] public string seasonDescription;
        [UIComponent("seasonImage")] public ImageView seasonImage;
        private Sprite seasonImageSprite;


        public SeasonListItem(int index, string seasonNumber, string seasonDescription, Sprite seasonImageSprite)
        {
            this.index = index;
            this.seasonNumber = seasonNumber;
            this.seasonDescription = seasonDescription;
            this.seasonImageSprite = seasonImageSprite;
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
