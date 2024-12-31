using Service.Entities;

namespace Service.Framework.Helpers.Context;

public static class DBConfigBase
{
  public static string? config(this MyContext db, string name, string group = "default")
  {
    var row = db.Options.FirstOrDefault(x => x.Name == name && x.Group == group);
    return row?.Value;
  }
}
