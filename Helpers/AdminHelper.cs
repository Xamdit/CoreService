using Blazored.LocalStorage;
using Global.Entities;
using Microsoft.AspNetCore.Components;
using Service.Core.Extensions;
using Service.Core.Synchronus;
using Service.Framework.Core.Engine;
using Service.Schemas;

namespace Service.Helpers;

public static class AdminHelper
{
  [Inject] private static ILocalStorageService Local { get; set; }

  public static bool has_permission(this HelperBase helper, string permission, string? staffid = null, string can = "")
  {
    return helper.staff_can(can, permission, Convert.ToInt32(staffid));
  }

  public static bool has_permission(this HelperBase helper, string permission, int? staffid = null, string can = "")
  {
    return helper.staff_can(can, permission, staffid).Result;
  }

  public static bool staff_can(this HelperBase helper, string view_own = "", string edit = "", string delete = "", string view = "", string create = "", string feature = "", int staffId = 0)
  {
    return helper.staff_can(edit, feature, staffId) ||
           helper.staff_can(delete, feature, staffId) ||
           helper.staff_can(view, feature, staffId) ||
           helper.staff_can(view_own, feature, staffId) ||
           helper.staff_can(create, feature, staffId);
  }

  public static bool staff_can(this HelperBase helper, string capability, string? feature = null, int staffId = 0)
  {
    return helper.staff_can(capability, feature, staffId);
  }

  public static async Task<bool> staff_can(this HelperBase helper, string capability, string? feature = null, int? staffId = null)
  {
    var (self, db) = getInstance();
    staffId ??= helper.get_staff_user_id();
    if (staffId.is_admin()) return true;
    var permissions = new List<StaffPermission>();
    if (!permissions.Any())
    {
      var staffModel = self.model.staff_model();
      permissions = await staffModel.get_staff_permissions(staffId);
    }

    // if (!string.IsNullOrEmpty(feature)) return Hooks.apply_filters("staff_can", permissions.Any(permission => feature == permission.feature && capability == permission.capability), capability, feature, staffId);
    var retVal = helper.in_array_multidimensional(permissions, "capability", capability);
    // return self.hooks.apply_filters("staff_can", retVal, capability, feature, staffId);
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
  //     staffid = self.helper.get_staff_user_id();
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

  public static bool is_admin(this object staffid)
  {
    var (self, db) = getInstance();
    var staff_id = Convert.ToInt32(staffid);

    if (staff_id > 0)
    {
      var user = Local.GetItemAsync<UserSchema>("user").Result;
      // if (isset(GLOBALS['current_user'])) return GLOBALS['current_user'].admin == '1';
      staffid = self.helper.get_staff_user_id();
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
