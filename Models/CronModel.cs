using Service.Entities;
using Service.Framework;

namespace Service.Models;

public class CronModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  public void run()
  {
  }
}
