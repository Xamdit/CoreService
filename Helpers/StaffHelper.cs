using System.Web;
using Global.Entities;
using Microsoft.AspNetCore.Components;
using Service.Core.Extensions;
using Service.Core.Synchronus;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;
using Service.Helpers.Tags;
using Service.Schemas;

namespace Service.Helpers;

public static class StaffHelper
{
  [Inject] private static AppObjectCache app_object_cache { get; set; }

  [Inject] private static SyncBuilder builder { get; set; }

  public static string get_staff_full_name(this HelperBase helper, int? userid = null)
  {
    var tmpStaffUserId = helper.get_staff_user_id();
    if (!userid.HasValue || userid == tmpStaffUserId)
    {
      if (CurrentUser != null)
        return CurrentUser.Fullname;
      userid = tmpStaffUserId;
    }

    var staff = app_object_cache.get<UserSchema>($"staff-full-name-data-{userid}");
    if (staff != null) return HttpUtility.HtmlEncode(staff.Firstname + " " + staff.Lastname);
    builder.where("staff_id", userid).from("users").select("firstname,lastname");
    // await builder.get_where("users", new { staff_id = userid }).row();
    builder.get_where("users", new { staff_id = userid });
    app_object_cache.add($"staff-full-name-data-{userid}", staff);
    return HttpUtility.HtmlEncode(staff != null ? staff.Firstname + " " + staff.Lastname : "");
  }

  /**
 * Check if user is staff member
 * In the staff profile there is option to check IS NOT STAFF MEMBER eq like contractor
 * Some features are disabled when user is not staff member
 * @param  string  $staff_id staff id
 * @return boolean
 */
  public static bool is_staff_member(this HelperBase helper)
  {
    var staff_id = helper.get_staff_user_id();
    return helper.is_staff_member(staff_id);
  }

  public static bool is_staff_member(this HelperBase helper, int staff_id)
  {
    var db = new MyContext();
    return db.Staff
      .Any(x => x.Id == staff_id && x.IsNotStaff == 0);
  }

  /**
 * Staff profile image with href
 * @param  boolean $id        staff id
 * @param  array   $classes   image classes
 * @param  string  $type
 * @param  array   $img_attrs additional <img /> attributes
 * @return string
 */
  public static string staff_profile_image(this HelperBase helper, int staff_id, string classes = null, string type = "small", Dictionary<string, string> imgAttrs = null)
  {
    return helper.staff_profile_image(staff_id, new List<string> { classes }, type, imgAttrs);
  }

  public static string staff_profile_image(this HelperBase helper, int staff_id, List<string> classes = null, string type = "small", Dictionary<string, string> imgAttrs = null)
  {
    var (self, db) = getInstance();
    var url = helper.site_url("assets/images/user-placeholder.jpg");
    var staff = new Staff();
    if (staff_id == helper.get_staff_user_id() && CurrentUser.Id > 0)
    {
      var user = CurrentUser;
      staff = db.Staff.FirstOrDefault(x => x.Id == user.Id);
    }
    else
    {
      staff = db.Staff.FirstOrDefault(x => x.Id == staff_id);
    }

    if (staff == null) return url;
    if (string.IsNullOrEmpty(staff.ProfileImage)) return url;
    var profileImagePath = "uploads/staff_profile_images/" + staff_id + "/" + type + "_" + staff.ProfileImage;
    if (helper.file_exists(profileImagePath)) url = helper.site_url(profileImagePath);

    return url;
  }

  /**
 * Get staff by ID or current logged in staff
 * @param  mixed $id staff id
 * @return mixed
 */
  public static Staff get_staff(this HelperBase helper, int? id = null)
  {
    var (self, db) = getInstance();
    var staff_model = self.model.staff_model();
    if (!id.HasValue && self.cache.has("current_user"))
    {
      var current_user = self.cache.get("current_user");
      if (current_user != null)
      {
        var rows = staff_model.get(x => x.Id == number(current_user));
        return rows.FirstOrDefault();
      }
    }

    // Staff not logged in
    return id.HasValue
      ? staff_model.get(x => x.Id == id.Value).FirstOrDefault()
      : null;
  }

  /**
 * Return staff profile image url
 * @param  mixed $staff_id
 * @param  string $type
 * @return string
 */
  public static string staff_profile_image_url(int staff_id, string type = "small")
  {
    var (self, db) = getInstance();
    var url = self.helper.base_url("assets/images/user-placeholder.jpg");
    var staff = staff_id == self.helper.get_staff_user_id() && self.globals<Staff?>("current_user") != null
      ? self.globals<Staff?>("current_user")
      : db.Staff.FirstOrDefault(x => x.Id == staff_id);
    if (staff == null) return url;
    if (string.IsNullOrEmpty(staff.ProfileImage)) return url;
    var profileImagePath = $"uploads/staff_profile_images/{staff_id}/{type}_{staff.ProfileImage}";
    if (self.file_exists(profileImagePath)) url = self.helper.base_url(profileImagePath);
    return url;
  }
}
