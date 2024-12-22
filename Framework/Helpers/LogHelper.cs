using Service.Framework.Core.Engine;

namespace Service.Framework.Helpers;

public static class LogHelper
{
  public static void log(this HelperBase helper, string message)
  {
    Console.WriteLine(message);
  }
}
