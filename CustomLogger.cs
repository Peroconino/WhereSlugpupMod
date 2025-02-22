using BepInEx.Logging;

namespace WhereSlugpupMod;
public class CustomLogger
{
  private readonly ManualLogSource _logger;
  private static bool ShouldLog => ModManager.DevTools;
  public CustomLogger()
  {
    _logger = Logger.CreateLogSource(WhereSlugpupMain.Name);
  }
  public void LogInfo(object data)
  {
    if (ShouldLog)
      _logger.LogInfo(data);
  }

  public void LogWarning(object data)
  {
    if (ShouldLog)
      _logger.LogWarning(data);
  }

  public void LogError(object data)
  {
    if (ShouldLog)
      _logger.LogError(data);
  }

  public void LogDebug(object data)
  {
    if (ShouldLog)
      _logger.LogDebug(data);
  }

  public void LogMessage(LogLevel level, object data)
  {
    if (ShouldLog)
      _logger.Log(level, data);
  }
}