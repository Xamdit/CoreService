using Global.Entities;
using Microsoft.EntityFrameworkCore;

namespace Service.Core.Extensions;

public static class EfcoreExtensions
{
  public static Global.Entities.Task task(this MyContext db, int id)
  {
    var output = db.Tasks
      .Include(x => x.TaskAssigneds)
      .Include(x => x.TaskComments)
      .Include(x => x.TaskChecklistItems)
      .Include(x => x.TaskFollowers)
      .Include(x => x.TasksTimers)
      .First(x => x.Id == id);
    return output;
  }

  public static Staff staff(this MyContext db, int id)
  {
    var output = db.Staff.FirstOrDefault(x => x.Id == id);
    return output ?? new Staff();
  }
}
