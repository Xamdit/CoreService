using Service.Framework;
using Service.Framework.Core.AppHook;

namespace Service.Core;

public class GlobalConstants
{
  public static MyInstance self = new();
  public static Hooks hooks = new();
}
