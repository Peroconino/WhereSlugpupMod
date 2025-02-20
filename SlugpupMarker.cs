using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HUD.Map;

namespace WhereSlugpupMod
{
    internal class SlugpupMarker : SlugcatMarker
    {
        public SlugpupMarker(Map map, int room, Vector2 inRoomPosition, Color slugcatColor) : base(map, room, inRoomPosition, slugcatColor)
        {

            //just switching color of bkg to white
            this.bkgFade.color = new Color(1f, 1f, 1f);

        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            //add more stuff here
        }

        public void PupDied()
        {
            this.symbolSprite.isVisible = false;
            Color temp = this.symbolSprite.color;
            this.symbolSprite = new FSprite("Multiplayer_Death", true);
            this.symbolSprite.color = temp;
            map.inFrontContainer.AddChild(this.symbolSprite);
            this.symbolSprite.isVisible = false;
        }

        public void PupMoved(int room)
        {
            this.room = room;
        }
    }
}
