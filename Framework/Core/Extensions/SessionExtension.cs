using Microsoft.EntityFrameworkCore;
using Service.Entities;

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

  public static void unset_userdata(this DbSet<Session> sessions, string key)
  {
  }

  public static bool has_userdata(this DbSet<Session> sessions, string url)
  {
    return false;
  }

  public static string get_userdata(this DbSet<Session> sessions, string url)
  {
    return string.Empty;
  }
}
