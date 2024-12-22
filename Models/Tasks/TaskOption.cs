namespace Service.Models.Tasks;

public class TaskOption
{
  public int id = TaskStatus.STATUS_NOT_STARTED;
  public string bg_color = "#333";
  public string color = "#64748b";
  public string text_color = "#333";
  public string name = "n/A";
  public int order = 1;
  public bool filter_default = true;
  public int CopyTaskStatus { get; set; } = 0;
  public bool copy_task_assignees { get; set; }
  public bool copy_task_followers { get; set; }
  public bool copy_task_checklist_items { get; set; }
  public bool copy_task_attachments { get; set; }
}
