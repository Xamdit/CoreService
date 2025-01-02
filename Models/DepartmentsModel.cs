using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;

namespace Service.Models;

public class DepartmentsModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  /**
       * @param  integer ID (optional)
       * @param  boolean (optional)
       * @return mixed
       * Get department object based on passed id if not passed id return array of all departments
       * Second parameter is to check if the request is coming from clientarea, so if any departments are hidden from client to exclude
       */
  public Department? get(int id, bool clientarea = false)
  {
    var query = db.Departments.AsQueryable();
    if (clientarea)
      query = query.Where(x => !x.HideFromClient);
    return query.FirstOrDefault(x => x.Id == id);
  }

  public List<Department> get(bool clientarea = false)
  {
    var query = db.Departments.AsQueryable();
    if (clientarea) query = query.Where(x => !x.HideFromClient);

    var departments = app_object_cache.get<List<Department>>("departments");

    if (departments != null) return departments;
    departments = query.ToList();
    app_object_cache.add("departments", departments);

    return departments;
  }

  /**
   * @param array _POST data
   * @return integer
   * Add new department
   */
  public int add(Department data)
  {
    if (!string.IsNullOrEmpty(data.Password)) data.Password = self.HashPassword(data.Password);

    data = hooks.apply_filters("before_department_added", data);
    db.Departments.Add(data);
    db.SaveChanges();

    var insert_id = data.Id;
    if (insert_id == 0) return insert_id;
    hooks.do_action("after_department_added", insert_id);
    log_activity("New Department Added [" + data.Name + ", ID: " + insert_id + "]");

    return insert_id;
  }

  /**
   * @param  array _POST data
   * @param  integer ID
   * @return boolean
   * Update department to database
   */
  public bool update(Department data)
  {
    var id = data.Id;
    var dep_original = get(id);
    if (dep_original == null) return false;

    if (string.IsNullOrEmpty(data.Password)) data.Encryption = "";

    if (string.IsNullOrEmpty(data.Email)) data.Email = null;

    // Check if not empty data['password']
    // Get original
    // Decrypt original
    // Compare with data['password']
    // If equal unset
    // If not encrypt and save
    if (!string.IsNullOrEmpty(data.Password)) data.Password = self.HashPassword(data.Password);
    data = hooks.apply_filters("before_department_updated", data);
    db.Departments.Update(data);
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity("Department Updated [Name: " + data.Name + ", ID: " + id + "]");
    return true;
  }

  /**
   * @param  integer ID
   * @return mixed
   * Delete department from database, if used return array with key referenced
   */
  public object delete(int id)
  {
    var current = get(id);

    if (db.is_reference_in_table<Department>("tickets", id))
      return new
      {
        referenced = true
      };

    hooks.do_action("before_delete_department", id);
    db.RemoveRange(db.StaffDepartments.Where(x => x.DepartmentId == id));
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity("Department Deleted [ID: " + id + "]");
    return true;
  }

  /**
   * @param  integer ID (option)
   * @param  boolean (optional)
   * @return mixed
   * Get departments where staff belongs
   * If onlyids passed return only departmentsID (simple array) if not returns array of all departments
   */
  public List<Department> get_staff_departments(int userid = 0)
  {
    var query = db.Departments.AsQueryable();
    if (userid == 0) userid = db.get_staff_user_id();
    var departments = query.Where(x => x.StaffDepartments.Any(y => y.StaffId == userid)).ToList();
    return departments;
  }

  public List<Department> get_departments()
  {
    return new List<Department>();
  }
}
