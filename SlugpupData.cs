
namespace WhereSlugpupMod;

public class SlugpupData(SlugpupMarker foundPupMarker)
{
  public SlugpupMarker FoundPupMarker { get; set; } = foundPupMarker;
  public bool IsNewPup { get; set; } = true;
  public bool IsMarkedOnTheMap { get; set; } = false;
}