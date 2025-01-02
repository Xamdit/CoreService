using Service.Entities;
using Service.Framework;

namespace Service.Helpers;

public static class CommonHelper
{
  public static string today()
  {
    return DateTime.UtcNow.ToString("Y-m-d H:i:s");
  }
}
