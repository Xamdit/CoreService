using Global.Entities;
using Service.Framework;

namespace Service.Models;

public class CronModel(MyInstance self, MyContext db) : MyModel(self)
{
  public void run()
  {
  }
}
