using System.Linq;
using UnityEngine;

namespace WhereSlugpupMod;

partial class WhereSlugpupMain
{
  private void CreateStaticPupsIcons()
  {
    if (whereSlugpupOptions.wantsPupMap.Value && SpawnedPups.unTammedPups.Count > 0 && SpawnedPups.uniqueMarkers.Count > 0)
    {
      var unTammedPups = SpawnedPups.unTammedPups;
      var uniqueMarkers = SpawnedPups.uniqueMarkers;
      for (int i = 0; i < uniqueMarkers.Count; i++)
      {
        var pup = unTammedPups.First(pup => pup.Value.FoundPupMarker.room == uniqueMarkers.ElementAt(i));
        if (pup.Key is not null && !pup.Value.IsMarkedOnTheMap)
        {
          pup.Key.world.game.cameras[0].hud.map.mapObjects.Add(unTammedPups[pup.Key].FoundPupMarker);
          pup.Value.IsMarkedOnTheMap = true;
          CustomLogger.LogInfo($"begining marker pos: {unTammedPups[pup.Key].FoundPupMarker.inRoomPos}");
        }
      }
    }
  }
  //Gets the slugpup's color from its ID and returns it
  private static Color SlugpupColor(AbstractCreature pupNPC)
  {

    Random.State state = Random.state;
    Random.InitState(pupNPC.ID.RandomSeed);

    //values needed for slugpup color
    Random.Range(0f, 1f); //meant to waste a value  
    float met = Mathf.Pow(Random.Range(0f, 1f), 1.5f);
    float stealth = Mathf.Pow(Random.Range(0f, 1f), 1.5f);
    Random.Range(0f, 1f); //meant to waste a value
    Random.Range(0f, 1f); //meant to waste a value
    float hue = Mathf.Lerp(Random.Range(0.15f, 0.58f), Random.value, Mathf.Pow(Random.value, 1.5f - met));
    float saturation = Mathf.Pow(Random.Range(0f, 1f), 0.3f + stealth * 0.3f);
    bool dark = Random.Range(0f, 1f) <= 0.3f + stealth * 0.2f;
    float luminosity = Mathf.Pow(Random.Range(dark ? 0.9f : 0.75f, 1f), 1.5f - stealth);

    Random.state = state;

    return RWCustom.Custom.HSL2RGB(hue, saturation, Mathf.Clamp(dark ? (1f - luminosity) : luminosity, 0.01f, 1f), 1f);
  }
}