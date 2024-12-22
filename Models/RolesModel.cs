using Global.Entities;
using Global.Entities.Tools;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Framework;
using Service.Models.Users;

namespace Service.Models;

public class RolesModel(MyInstance self, MyContext db) : MyModel(self)
{
  private StaffModel staff_model = self.model.staff_model();

  /**
* Add new employee role
* @param mixed data
*/
  public int add(Role data)
  {
    var permissions = new List<string?>();
    if (!string.IsNullOrEmpty(data.Permissions)) permissions = JsonConvert.DeserializeObject<List<string>>(data.Permissions);

    data.Permissions = JsonConvert.SerializeObject(permissions);
    db.Roles.Add(data);
    db.SaveChanges();
    var insert_id = data.Id;
    if (insert_id <= 0) return 0;
    log_activity("New Role Added [ID: " + insert_id + "." + data.Name + "]");
    return insert_id;
  }

  /**
   * Update employee role
   * @param  array data role data
   * @param  mixed id   role id
   * @return boolean
   */
  public async Task<bool> update(Role data)
  {
    var id = data.Id;
    var affectedRows = 0;
    var permissions = new List<StaffPermission>();
    if (!string.IsNullOrEmpty(data.Permissions)) permissions = JsonConvert.DeserializeObject<List<StaffPermission>>(data.Permissions);

    data.Permissions = JsonConvert.SerializeObject(permissions);

    var update_staff_permissions = false;
    // if (isset(data['update_staff_permissions']))
    // {
    //   update_staff_permissions = true;
    //   unset(data['update_staff_permissions']);
    // }

    db.Roles.Where(x => x.Id == id).Update(x => data);
    var affected_rows = db.SaveChanges();

    if (affected_rows > 0) affectedRows++;

    if (update_staff_permissions)
    {
      var staff = staff_model.get(x => x.Role == id);
      affectedRows += staff.Count(member => staff_model.update_permissions(permissions, member.Id).Result);
    }

    if (affectedRows <= 0) return false;
    log_activity("Role Updated [ID: " + id + ", Name: " + data.Name + "]");
    return true;
  }

  /**
   * Get employee role by id
   * @param  mixed id Optional role id
   * @return mixed     array if not id passed else object
   */
  public List<Role> get()
  {
    return db.Roles.ToList();
  }

  public Role? get(int id)
  {
    var role = app_object_cache.get<Role>("role-" + id);
    if (role == null) return role;
    role = db.Roles.FirstOrDefault(x => x.Id == id);
    role.Permissions = JsonConvert.SerializeObject(!string.IsNullOrEmpty(role.Permissions) ? role.Permissions : new List<object>());
    app_object_cache.add($"role-{id}", role);
    return role;
  }

  /**
   * Delete employee role
   * @param  mixed id role id
   * @return mixed
   */
  public object delete(int id)
  {
    var current = get(id);
    // Check first if role is used in table
    // if (db.is_reference_in_table<Role>('role', 'staff', id))
    if (db.is_reference_in_table<Role>("staff", id)) return new { referenced = true };
    var affectedRows = 0;
    db.Roles.RemoveRange(db.Roles.Where(x => x.Id == id));
    var affected_rows = db.SaveChanges();
    if (affected_rows > 0) affectedRows++;
    if (affectedRows <= 0) return false;
    log_activity("Role Deleted [ID: " + id + "]");
    return true;
  }

  public List<ContactPermission> get_contact_permissions(int id)
  {
    var rows = db.ContactPermissions.Where(x => x.UserId == id).ToList();
    return rows;
  }

  public List<Staff> get_role_staff(int role_id)
  {
    var rows = db.Staff.Where(x => x.Role == role_id).ToList();
    return rows;
  }
}
