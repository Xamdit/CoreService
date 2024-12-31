using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Core.Engine;

public class HelperBaseExtension
{
  public static (MyInstance self, MyContext db) getInstance()
  {
    var output = MyInstance.Instance;
    var database = output.db();
    return (output, database);
  }
}
