
namespace WhereSlugpupMod;

public class SlugpupData(SlugpupMarker foundPupMarker, bool isNewPup = true, bool isMarkedOnTheMap = false)
{
  public SlugpupMarker FoundPupMarker { get; set; } = foundPupMarker;
  public bool IsNewPup { get; set; } = isNewPup;
  public bool IsMarkedOnTheMap { get; set; } = isMarkedOnTheMap;
}