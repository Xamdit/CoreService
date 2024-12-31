using Service.Entities;
using Service.Framework;

namespace Service.Helpers;

public static class CommonHelper
{
  public static MyContext db(this MyInstance self)
  {
    return new MyContext();
  }

  public static string today()
  {
    return DateTime.UtcNow.ToString("Y-m-d H:i:s");
  }
}
