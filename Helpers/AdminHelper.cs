using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Service.Core.Synchronus;
using Service.Entities;
using Service.Schemas;

namespace Service.Helpers;

public static class AdminHelper
{
  [Inject] private static ILocalStorageService Local { get; set; }

  public static bool has_permission(this MyContext db, string permission, string? staffid = null, string can = "")
  {
    return db.staff_can(can, permission, Convert.ToInt32(staffid));
  }

  public static bool has_permission(this MyContext db, string permission, int? staffid = null, string can = "")
  {
    return db.staff_can(can, permission, staffid).Result;
  }

  public static bool staff_can(this MyContext db, string view_own = "", string edit = "", string delete = "", string view = "", string create = "", string feature = "", int staffId = 0)
  {
    return db.staff_can(edit, feature, staffId) ||
           db.staff_can(delete, feature, staffId) ||
           db.staff_can(view, feature, staffId) ||
           db.staff_can(view_own, feature, staffId) ||
           db.staff_can(create, feature, staffId);
  }

  public static bool staff_can(this MyContext db, string capability, string? feature = null, int staffId = 0)
  {
    return db.staff_can(capability, feature, staffId);
  }

  private static async Task<List<StaffPermission>> get_staff_permissions(int? staffId)
  {
    var result = await syncBuilder.get_where("/users/permissions/{staffId}");
    return result.Success ? JsonConvert.DeserializeObject<List<StaffPermission>>(result.Content) : default;
  }

  public static async Task<bool> staff_can(this MyContext db, string capability, string? feature = null, int? staffId = null)
  {
    staffId ??= db.get_staff_user_id();
    if (db.is_admin()) return true;
    var permissions = new List<StaffPermission>();
    if (!permissions.Any()) permissions = await get_staff_permissions(staffId);

    var retVal = db.in_array_multidimensional(permissions, "capability", capability);
    // return hooks.apply_filters("staff_can", retVal, capability, feature, staffId);
    return false;
  }

  // public static bool is_admin(this object staffid )
  // {
  //   var self = Constants.Instance;
  //
  //   if (staffid != null)
  //   {
  //     var user = Local.GetItemAsync<UserSchema>("user").Result;
  //     // if (isset(GLOBALS['current_user'])) return GLOBALS['current_user'].admin == '1';
  //     staffid = db.get_staff_user_id();
  //   }
  //
  //   var cache = app_object_cache.get<string>($"is-admin-{staffid}");
  //   // if (cache != null) return cache.Data == "yes";
  //
  //   // if (app_object_cache.get($"is-admin-{staffid}", out var cache, ""))
  //   //   return cache == "yes";
  //
  //   var result = syncBuilder.get_where("users", new { admin = 1, staff_id = staffid }).Result;
  //
  //   // var result = db.count_all_results("staff") > 0;
  //   // app_object_cache.add($"is-admin-{staffid}", result ? "yes" : "no");
  //
  //   return self.helper.is_admin(staffid);
  // }
  // read from token
  public static bool is_admin(this MyContext db)
  {
    return false;
  }

  public static bool is_admin(this MyContext db, int staffid)
  {
    var staff_id = Convert.ToInt32(staffid);

    if (staff_id > 0)
    {
      var user = Local.GetItemAsync<UserSchema>("user").Result;
      // if (isset(GLOBALS['current_user'])) return GLOBALS['current_user'].admin == '1';
      staffid = db.get_staff_user_id();
    }

    var cache = app_object_cache.get<string>($"is-admin-{staffid}");
    // if (cache != null) return cache.Data == "yes";

    // if (app_object_cache.get($"is-admin-{staffid}", out var cache, ""))
    //   return cache == "yes";

    var result = syncBuilder.get_where("users", new { admin = 1, staff_id = staffid }).Result;

    // var result = db.count_all_results("staff") > 0;
    // app_object_cache.add($"is-admin-{staffid}", result ? "yes" : "no");

    // return self.helper.is_admin(staff_id);
    return false;
  }
}
