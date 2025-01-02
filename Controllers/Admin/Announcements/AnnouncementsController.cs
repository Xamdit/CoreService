using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Models;

namespace Service.Controllers.Admin.Announcements;

[ApiController]
[Route("api/admin/announcements")]
public class AnnouncementsController(ILogger<AnnouncementsController> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{
  public AnnouncementsModel announcements_model { get; set; }

  public override void Init()
  {
    announcements_model = self.announcements_model(db);
  }

  /* List all announcements */
  public IActionResult index()
  {
    // if (self.input.is_ajax_request())
    //   this.app.get_table_data("announcements");
    data.title = label("announcements");
    return MakeResult(data);
  }

  /* Edit announcement or add new if passed id */
  [HttpGet("announcement/{id:int?}")]
  public IActionResult announcement_get(int? id = null)
  {
    if (!db.is_admin()) access_denied("Announcement");
    var title = string.Empty;
    if (!id.HasValue)
    {
      title = label("add_new", label("announcement_lowercase"));
    }
    else
    {
      data.announcement = announcements_model.FindOne(id.Value);
      title = label("edit", label("announcement_lowercase"));
    }

    data.title = title;
    return MakeResult(data);
  }

  [HttpPost("announcement/{id:int?}")]
  public IActionResult announcement(int? id)
  {
    if (!db.is_admin()) access_denied("Announcement");
    var dataset = self.input.post<Announcement>();
    dataset.Message = self.input.post("message");
    if (!id.HasValue)
    {
      id = announcements_model.add(data);
      if (id > 0)
      {
        set_alert("success", label("added_successfully", label("announcement")));
        return Redirect(admin_url("announcements/view/" + id));
      }
    }
    else
    {
      dataset.Id = id.Value;
      var success = announcements_model.update(dataset);
      if (success) set_alert("success", label("updated_successfully", label("announcement")));
      return Redirect(admin_url("announcements/view/" + id));
    }

    return Ok();
  }


  public IActionResult view(int id)
  {
    if (!db.is_staff_member()) return Ok();
    var announcement = announcements_model.get(id);
    if (announcement == null)
      return MakeError(label("announcement_not_found"));
    data.announcement = announcement;
    data.recent_announcements = announcements_model.get(x => x.Id != id, 4);
    data.title = announcement.Name;
    return MakeResult(data);
  }

  /* Delete announcement from database */
  [HttpDelete("{id:int?}")]
  public IActionResult delete(int? id)
  {
    if (!id.HasValue)
      return Redirect(admin_url("announcements"));
    if (!db.is_admin())
      access_denied("Announcement");
    var response = announcements_model.delete(id.Value);
    if (response == true)
      set_alert("success", label("deleted", label("announcement")));
    else
      set_alert("warning", label("problem_deleting", label("announcement_lowercase")));
    // return Redirect(_SERVER["HTTP_REFERER"]);
    return Redirect("HTTP_REFERER");
  }
}
