using Service.Entities;

namespace Service.Helpers.Database;

public static class SingleEntities
{
  public static TaskAssigned task_assigned(this MyContext db, TaskAssigned taskAssigned)
  {
    return db.TaskAssigneds.First(x => x.Id == taskAssigned.Id);
  }
}
