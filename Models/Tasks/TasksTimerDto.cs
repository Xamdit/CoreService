using Global.Entities;

namespace Service.Models.Tasks;

public class TasksTimerDto : TasksTimer
{
  public int timesheet_staff_id { get; set; } = 0;
  public int timesheet_task_id { get; set; } = 0;
  public List<Taggable> Tags { get; set; } = new();
}
