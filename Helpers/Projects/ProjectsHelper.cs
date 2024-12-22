using Global.Entities;
using Service.Core.Extensions;
using Service.Framework.Core.Engine;
using Service.Models.Projects;
using Service.Schemas.Ui.Entities;

namespace Service.Helpers.Projects;

public static class ProjectsHelper
{
  /**
 * Get project client id by passed project id
 * @param  mixed id project id
 * @return mixed
 */
  public static int get_client_id_by_project_id(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var project = db.Projects.FirstOrDefault(x => x.Id == id);
    return project?.ClientId ?? 0;
  }

  /**
 * Get project billing type
 * @param  mixed $project_id
 * @return mixed
 */
  public static int get_project_billing_type(this HelperBase helper, int project_id)
  {
    var (self, db) = getInstance();
    var project = db.Projects.Where(x => x.Id == project_id).FirstOrDefault();
    return project?.BillingType ?? 0;
  }

  /**
 * Default project tabs
 * @return array
 */
  public static List<Tab> get_project_tabs_admin(this HelperBase helper)
    // public static List<string> get_project_tabs_admin(this HelperBase helper)
  {
    // return get_instance()->app_tabs->get_project_tabs();
    return default;
  }

  public static string get_project_name_by_id(this MyContext db, int id)
  {
    var project = db.Projects.FirstOrDefault(x => x.Id == id);
    return project?.Name ?? "";
  }

  /**
 * Get project status by passed project id
 * @param  mixed $id project id
 * @return array
 */
  public static ProjectSettingOption get_project_status_by_id(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var projects_model = self.model.projects_model();
    var statuses = projects_model.get_project_statuses();

    var status = new ProjectSettingOption()
    {
      Id = 0,
      Color = "#333",
      Name = "[Status Not Found]",
      Order = 1
    };

    foreach (var s in statuses.Where(s => s.Id == id))
    {
      status = s;
      break;
    }

    return status;
  }

  public static List<Global.Entities.File> get_project_files(this HelperBase helperBase, int project_id)
  {
    return new List<Global.Entities.File>();
  }

  public static List<Project> view_tasks(this HelperBase helperBase, List<string> args)
  {
    return new List<Project>();
  }
}
