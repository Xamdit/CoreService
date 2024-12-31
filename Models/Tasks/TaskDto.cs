using Service.Entities;
using File = Service.Entities.File;
using Task = Service.Entities.Task;

namespace Service.Models.Tasks;

public class TaskDto : Task
{
  public List<Taggable> Tags { get; set; } = new();
  public List<int> assignees_ids { get; set; } = new();
  public List<TaskFollower> followers { get; set; }
  public List<int> followers_ids { get; set; }
  public List<File> attachments { get; set; }
  public List<TasksTimer> timesheets { get; set; }
  public bool current_user_is_assigned { get; set; }
  public bool current_user_is_creator { get; set; }
  public int? hide_milestone_from_customer { get; set; }
  public List<Project> project_data { get; set; }
  public string repeat_every_custom { get; set; }
  public string repeat_type_custom { get; set; }
  public List<CustomField> custom_fields { get; set; } = new();
}
