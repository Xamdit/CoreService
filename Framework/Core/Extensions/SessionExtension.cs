using Global.Entities;
using Microsoft.EntityFrameworkCore;

namespace Service.Framework.Core.Extensions;

public static class SessionExtension
{
  public static string get_string(this DbSet<Session> sessions, string key)
  {
    return string.Empty;
  }

  public static void set_userdata(this DbSet<Session> sessions, string key, object value)
  {
  }
}