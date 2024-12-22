using Global.Entities;

namespace Service.Helpers;

public static class ModelHelper
{
  public static bool view_milestone(this MyContext db, IEnumerable<ProjectSetting> projectSettings)
  {
    var _projectSettings = projectSettings.ToList().Select(x => x.Id).ToList();
    return db.Milestones.Any(x => _projectSettings.Contains(x.Id));
  }
}
