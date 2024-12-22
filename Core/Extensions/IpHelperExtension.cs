using Service.Framework;

namespace Service.Core.Extensions;

public static class IpHelperExtension
{
  public static string ip(this MyInstance self)
  {
    var ip = self.context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (string.IsNullOrEmpty(ip)) ip = self.context.Connection.RemoteIpAddress.ToString();
    return ip;
  }
}
