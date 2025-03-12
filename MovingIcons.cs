using System.Collections.Generic;
using System.Reflection;
using HUD;
using MoreSlugcats;
using UnityEngine;

namespace WhereSlugpupMod;

partial class WhereSlugpupMain
{
  private void CreateMovingPupsIcons(Map self, float timeStacker)
  {
    if (whereSlugpupOptions.wantsEnhancedSlugpupAwareness.Value && self.fade > 0f)
    {
      List<AbstractCreature?> slugcats = [.. SpawnedPups.unTammedPups.Keys, .. SpawnedPups.tammedPups.Keys];

      FieldInfo creatureSymbolsField = typeof(Map).GetField("creatureSymbols", BindingFlags.NonPublic | BindingFlags.Instance);
      List<CreatureSymbol> creatureSymbols = (List<CreatureSymbol>)creatureSymbolsField.GetValue(self);// needed to use reflection here to access creatureSymbols

      foreach (CreatureSymbol creatureSymbol in creatureSymbols)
      {
        if (creatureSymbol.iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
          creatureSymbol.RemoveSprites();
        }
      }

      foreach (AbstractCreature? slugcat in slugcats)
      {
        if (slugcat is not null)
          CreateSlugpupSymbol(self, slugcat, creatureSymbols, timeStacker);
      }
      slugcats.Clear();
    }
  }
  private static void CreateSlugpupSymbol(Map self, AbstractCreature slugcat, List<CreatureSymbol> creatureSymbols, float timeStacker)
  {
    if (slugcat.pos.TileDefined)
    {
      CreatureSymbol slugPupSymbol = new(CreatureSymbol.SymbolDataFromCreature(slugcat), self.inFrontContainer);
      slugPupSymbol.Show(true);
      slugPupSymbol.lastShowFlash = 0f;
      slugPupSymbol.showFlash = 0f;
      slugPupSymbol.myColor = SlugpupColor(slugcat);
      slugPupSymbol.symbolSprite.alpha = 0.9f;

      if (slugcat.realizedCreature == null || slugcat.realizedCreature.dead)
      {
        slugPupSymbol.symbolSprite.scale = 0.8f;
        slugPupSymbol.symbolSprite.alpha = 0.7f;
      }
      slugPupSymbol.shadowSprite1.alpha = slugPupSymbol.symbolSprite.alpha;
      slugPupSymbol.shadowSprite2.alpha = slugPupSymbol.symbolSprite.alpha;
      slugPupSymbol.shadowSprite1.scale = slugPupSymbol.symbolSprite.scale;
      slugPupSymbol.shadowSprite2.scale = slugPupSymbol.symbolSprite.scale;
      Vector2 drawPos = self.RoomToMapPos((slugcat.realizedCreature == null) ? (slugcat.pos.Tile.ToVector2() * 20f) : slugcat.realizedCreature.mainBodyChunk.pos, slugcat.Room.index, timeStacker);
      slugPupSymbol.Draw(timeStacker, drawPos);
      creatureSymbols.Add(slugPupSymbol);
    }

  }
}