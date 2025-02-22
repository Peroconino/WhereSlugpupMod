using HUD;
using UnityEngine;
using static HUD.Map;

namespace WhereSlugpupMod;

public class SlugpupMarker : SlugcatMarker
{
    public SlugpupMarker(Map map, int room, Vector2 inRoomPosition, Color slugcatColor) : base(map, room, inRoomPosition, slugcatColor)
    {

        //just switching color of bkg to white
        bkgFade.color = new Color(1f, 1f, 1f);

    }

    public void PupDied()
    {
        symbolSprite.isVisible = false;
        Color temp = symbolSprite.color;
        symbolSprite = new FSprite("Multiplayer_Death", true)
        {
            color = temp
        };
        map.inFrontContainer.AddChild(symbolSprite);
        symbolSprite.isVisible = false;
    }

    public void PupMoved(int puproom)
    {
        room = puproom;
    }
}

