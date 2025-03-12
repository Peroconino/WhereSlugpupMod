using System.Collections.Generic;

namespace WhereSlugpupMod;

public class SpawnedPups
{
  public readonly Dictionary<AbstractCreature?, SlugpupData> unTammedPups = [];
  public readonly Dictionary<AbstractCreature?, SlugpupData> tammedPups = [];
  public readonly HashSet<int> uniqueMarkers = []; //hashset is better to guarantee theres no duplicates
  public void Clear()
  {
    unTammedPups.Clear();
    tammedPups.Clear();
    uniqueMarkers.Clear();
  }

  public bool Remove(AbstractCreature creature)
  {
    if (unTammedPups.ContainsKey(creature))
    {
      unTammedPups.Remove(creature);
      return true;
    }
    if (tammedPups.ContainsKey(creature))
    {
      tammedPups.Remove(creature);
      return true;
    }
    return false;
  }

  public bool ContainsKey(AbstractCreature creature)
  {
    return unTammedPups.ContainsKey(creature) || tammedPups.ContainsKey(creature);
  }
}