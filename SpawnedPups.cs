using System.Collections.Generic;

namespace WhereSlugpupMod;

public class SpawnedPups
{
  public readonly Dictionary<AbstractCreature, SlugpupData> unTammedPups = [];
  public readonly Dictionary<AbstractCreature, SlugpupData> tammedPups = [];
  public readonly HashSet<int> uniqueMarkers = []; //hashset is better to guarantee theres no duplicates
  public void Clear()
  {
    unTammedPups.Clear();
    tammedPups.Clear();
    uniqueMarkers.Clear();
  }
}