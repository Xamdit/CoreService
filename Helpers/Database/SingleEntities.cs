using Service.Entities;

namespace Service.Helpers.Database;

public static class SingleEntities
{
  public static TaskAssigned task_assigned(this MyContext db, int id)
  {
    return db.TaskAssigneds.First(x => x.Id == id);
  }
}
