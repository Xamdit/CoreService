using Global.Entities;

namespace Service.Models.Projects;

public class ProjectDto : Project
{
  public bool project_marked_as_finished_email_to_contacts { get; set; }
  public bool send_created_email { get; set; }
  public bool notify_project_members_status_change { get; set; }
  public List<CustomField> custom_fields { get; set; } = new();
}
