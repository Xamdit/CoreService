namespace Service.Framework.Core.AppHook;

public class AppHooks
{
  public bool apply_filters<T>(string staffCan, bool p1, string capability, string? feature, int? staffId) where T : class
  {
    return false;
  }
}
