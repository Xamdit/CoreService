using Service.Entities;

namespace Service.Models.Users;

public class LoggedTime
{
  public List<TasksTimer> timesheets = new();
  public List<long> total = new();
  public List<long> this_month = new();
  public List<long> last_month = new();
  public List<long> this_week = new();
  public List<long> last_week = new();
}

public class LoggedTimeData
{
  public float total { get; set; }
  public long this_month { get; set; }
  public long last_month { get; set; }
  public long this_week { get; set; }
  public long last_week { get; set; }
}
