using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Core.Synchronus;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Helpers;
using File = Service.Entities.File;
using Task = Service.Entities.Task;


namespace Service.Models.Users;

public class StaffModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private DepartmentsModel departments_model = self.departments_model(db);

  public async Task<List<StaffPermission>> get_staff_permissions(int? staffId)
  {
    var result = await syncBuilder.get_where("/users/permissions/{staffId}");
    return result.Success ? JsonConvert.DeserializeObject<List<StaffPermission>>(result.Content) : default;
  }

  public bool delete(int id, int transfer_data_to)
  {
    if (id == transfer_data_to) return false;

    hooks.do_action("before_delete_staff_member", new { id, transfer_data_to });

    var name = db.get_staff_full_name(id);
    var transferred_to = db.get_staff_full_name(transfer_data_to);

    db.Estimates
      .Where(x => x.AddedFrom == id)
      .Update(x => new Estimate { AddedFrom = transfer_data_to });

    db.Estimates
      .Where(x => x.SaleAgent == id)
      .Update(x => new Estimate { SaleAgent = transfer_data_to });

    db.Invoices
      .Where(x => x.AddedFrom == id)
      .Update(x => new Invoice { AddedFrom = transfer_data_to });

    db.Invoices
      .Where(x => x.SaleAgent == id)
      .Update(x => new Invoice { SaleAgent = transfer_data_to });

    db.Expenses
      .Where(x => x.AddedFrom == id)
      .Update(x => new Expense { AddedFrom = transfer_data_to });

    db.Notes
      .Where(x => x.AddedFrom == id)
      .Update(x => new Note { AddedFrom = transfer_data_to });

    db.NewsfeedPostComments
      .Where(x => x.UserId == id)
      .Update(x => new NewsfeedPostComment { UserId = transfer_data_to });

    db.NewsfeedPosts
      .Where(x => x.Creator == id)
      .Update(x => new NewsfeedPost { Creator = transfer_data_to });

    db.ProjectDiscussions
      .Where(x => x.StaffId == id)
      .Update(x => new ProjectDiscussion { StaffId = transfer_data_to });

    db.Projects
      .Where(x => x.AddedFrom == id)
      .Update(x => new Project { AddedFrom = transfer_data_to });

    db.CreditNotes
      .Where(x => x.AddedFrom == id)
      .Update(x => new CreditNote { AddedFrom = transfer_data_to });

    db.Credits
      .Where(x => x.StaffId == id)
      .Update(x => new Credit { StaffId = transfer_data_to });

    db.ProjectFiles
      .Where(x => x.StaffId == id)
      .Update(x => new ProjectFile { StaffId = transfer_data_to });

    db.ProposalComments
      .Where(x => x.StaffId == id)
      .Update(x => new ProposalComment { StaffId = transfer_data_to });

    db.Proposals
      .Where(x => x.AddedFrom == id)
      .Update(x => new Proposal { AddedFrom = transfer_data_to });

    db.Templates
      .Where(x => x.AddedFrom == id)
      .Update(x => new Template { AddedFrom = transfer_data_to });

    db.TaskComments
      .Where(x => x.StaffId == id)
      .Update(x => new TaskComment { StaffId = transfer_data_to });

    db.Tasks
      .Where(x => x.AddedFrom == id && x.IsAddedFromContact == false)
      .Update(x => new Task { AddedFrom = transfer_data_to });

    db.Files
      .Where(x => x.StaffId == id)
      .Update(x => new File { StaffId = transfer_data_to });

    db.ContractRenewals
      .Where(x => x.RenewedByStaffId == id)
      .Update(x => new ContractRenewal { RenewedByStaffId = transfer_data_to });

    db.TaskChecklistItems
      .Where(x => x.AddedFrom == id)
      .Update(x => new TaskChecklistItem { AddedFrom = transfer_data_to });

    db.TaskChecklistItems
      .Where(x => x.Assigned == id)
      .Update(x => new TaskChecklistItem { Assigned = transfer_data_to });

    db.TaskChecklistItems
      .Where(x => x.FinishedFrom == id)
      .Update(x => new TaskChecklistItem { FinishedFrom = transfer_data_to });

    db.TicketReplies
      .Where(x => x.Admin == id)
      .Update(x => new TicketReply { Admin = transfer_data_to });

    db.Tickets
      .Where(x => x.Admin == id)
      .Update(x => new Ticket { Admin = transfer_data_to });

    db.Leads
      .Where(x => x.AddedFrom == id)
      .Update(x => new Lead { AddedFrom = transfer_data_to });

    db.Leads
      .Where(x => x.Assigned == id)
      .Update(x => new Lead { Assigned = transfer_data_to });

    db.TasksTimers
      .Where(x => x.StaffId == id)
      .Update(x => new TasksTimer { StaffId = transfer_data_to });

    db.Contracts
      .Where(x => x.AddedFrom == id)
      .Update(x => new Contract { AddedFrom = transfer_data_to });

    db.TaskAssigneds
      .Where(x => x.AssignedFrom == id && x.IsAssignedFromContact == 0)
      .Update(x => new TaskAssigned { AssignedFrom = transfer_data_to });

    db.LeadsEmailIntegrations
      .Where(x => x.Responsible == id)
      .Update(x => new LeadsEmailIntegration { Responsible = transfer_data_to });

    db.WebToLeads
      .Where(x => x.Responsible == id)
      .Update(x => new WebToLead { Responsible = transfer_data_to });

    db.EstimateRequestForms
      .Where(x => x.Responsible == id)
      .Update(x => new EstimateRequestForm { Responsible = transfer_data_to });

    db.EstimateRequests
      .Where(x => x.Assigned == id)
      .Update(x => new EstimateRequest { Assigned = transfer_data_to });


    db.Subscriptions
      .Where(x => x.CreatedFrom == id)
      .Update(x => new Subscription { CreatedFrom = transfer_data_to });

    var web_to_lead = db.WebToLeads.Where(x => x.NotifyType == "specific_staff").ToList();

    web_to_lead.ForEach(form =>
    {
      if (string.IsNullOrEmpty(form.NotifyIds)) return;
      var staff = JsonConvert.DeserializeObject<List<int>>(form.NotifyIds);
      if (staff.Contains(id)) db.WebToLeads.Where(x => x.Id == form.Id).Update(x => new WebToLead { NotifyIds = JsonConvert.SerializeObject(staff.Where(x => x != id).ToList()) });
    });
    db.SaveChanges();
    var estimate_requests = db.EstimateRequestForms.ToList();


    db.EstimateRequestForms
      .Where(x => x.NotifyType == "specific_staff")
      .ToList()
      .ForEach(form =>
      {
        if (string.IsNullOrEmpty(form.NotifyIds)) return;
        var staff = JsonConvert.DeserializeObject<List<int>>(form.NotifyIds);
        if (staff.Contains(id)) db.EstimateRequestForms.Where(x => x.Id == form.Id).Update(x => new EstimateRequestForm { NotifyIds = JsonConvert.SerializeObject(staff.Where(x => x != id).ToList()) });
      });
    db.SaveChanges();
    var leads_email_integration = db.LeadsEmailIntegrations.First(x => x.Id == 1);
    if (leads_email_integration.NotifyType == "specific_staff")
      if (!string.IsNullOrEmpty(leads_email_integration.NotifyIds))
      {
        // Deserialize the NotifyIds into a list of integers or strings, depending on your data structure
        var staff = JsonConvert.DeserializeObject<List<int>>(leads_email_integration.NotifyIds);

        // Check if the staff list contains the id, and find the index of that id
        if (staff != null && staff.Contains(id))
        {
          // Find the index of the id
          var index = staff.IndexOf(id);
          if (index != -1)
          {
            // Remove the id from the staff list
            staff.RemoveAt(index);

            // Serialize the updated staff list back to a JSON string
            var updatedNotifyIds = JsonConvert.SerializeObject(staff);

            // Update the NotifyIds field in the database
            db.LeadsEmailIntegrations
              .Where(x => x.Id == leads_email_integration.Id) // Assuming you're updating the current leads_email_integration
              .Update(x => new LeadsEmailIntegration { NotifyIds = updatedNotifyIds });
            // Save the changes to the database
            db.SaveChanges();
          }
        }
      }

    db.Tickets
      .Where(x => x.Assigned == id)
      .Update(x => new Ticket { Assigned = 0 });

    db.DismissedAnnouncements.Remove(new DismissedAnnouncement { IsStaff = true, UserId = id });
    db.NewsfeedCommentLikes.Remove(new NewsfeedCommentLike { UserId = id });

    db.NewsfeedPostLikes
      .Where(x => x.UserId == id)
      .Delete();

    db.CustomerAdmins
      .Where(x => x.StaffId == id)
      .Delete();

    db.CustomFieldsValues
      .Where(x =>
        x.FieldTo == "staff" &&
        x.RelId == id
      )
      .Delete();

    db.Events
      .Where(x => x.UserId == id)
      .Delete();

    db.Notifications.Remove(new Notification { ToUserId = id });

    db.UserMeta
      .Where(x => x.StaffId == id)
      .Delete();

    db.UserMeta
      .Where(x => x.ClientId == id)
      .Delete();

    db.ProjectMembers
      .Where(x => x.StaffId == id)
      .Delete();

    db.ProjectNotes
      .Where(x => x.StaffId == id)
      .Delete();

    db.Reminders
      .Where(x => x.Creator == id || x.StaffId == id)
      .Delete();

    db.StaffDepartments
      .Where(x => x.StaffId == id)
      .Delete();

    db.Todos
      .Where(x => x.StaffId == id)
      .Delete();

    db.UserAutoLogins
      .Where(x => x.IsStaff && x.UserId == id)
      .Delete();

    db.StaffPermissions
      .Where(x => x.StaffId == id)
      .Delete();

    db.TaskAssigneds
      .Where(x => x.StaffId == id)
      .Delete();

    db.TaskFollowers
      .Where(x => x.StaffId == id)
      .Delete();

    db.PinnedProjects
      .Where(x => x.StaffId == id)
      .Delete();

    db.Staff
      .Where(x => x.Id == id)
      .Delete();

    db.SaveChanges();
    log_activity($"Staff Member Deleted [Name: {name}, Data Transferred To: {transferred_to}]");
    hooks.do_action("staff_member_deleted", new { id, transfer_data_to });
    return true;
  }

  /**
   * Get staff member/s
   * @param  mixed  id Optional - staff id
   * @param  mixed where where in query
   * @return mixed if id is passed return object else array
   */
  // Method to get a list of staff members based on a condition (where clause)
  public List<Staff> get(Expression<Func<Staff, bool>> where)
  {
    // Call the other get method with id = 0 to signify that you're fetching all staff members.
    var rows = db.Staff
      .Where(where)
      .OrderByDescending(s => s.FirstName)
      .ToList();
    return rows;
  }

  // Method to get a specific staff member by id, including additional details
  public async Task<Staff?> get(int id, Expression<Func<Staff, bool>> where)
  {
    // Query to select staff including the full name as a concatenated field

    var isStaffLoggedIn = db.is_staff_logged_in();

    // Create a query with conditional fields for notifications and todos if the user is logged in
    var query = db.Staff
      .Where(where)
      .Select(staff => new
      {
        Staff = staff,
        FullName = $"{staff.FirstName} {staff.LastName}",
        TotalUnreadNotifications = isStaffLoggedIn && id != 0 && id == staff_user_id
          ? db.Notifications.Count(n => n.ToUserId == id && !n.IsRead)
          : (int?)null,
        TotalUnfinishedTodos = isStaffLoggedIn && id != 0 && id == staff_user_id
          ? db.Todos.Count(t => t.StaffId == id && t.Finished != 0)
          : (int?)null
      });

    // Check if we are fetching a specific staff member by id
    if (id == 0 || id != staff_user_id)
      return await db.Staff
        .Where(where)
        .OrderByDescending(s => s.FirstName)
        .FirstOrDefaultAsync();

    var staffWithNotifications = query.FirstOrDefault(staff => staff.Staff.Id == id);
    if (staffWithNotifications == null)
      return await db.Staff
        .Where(where)
        .OrderByDescending(s => s.FirstName)
        .FirstOrDefaultAsync();

    var staff = staffWithNotifications.Staff;
    staff.StaffPermissions = get_staff_permissions(id); // Assuming you have a method for fetching permissions
    return staff;


    // Fallback: If no specific id or no matching staff, return the first matching staff member
  }


  /**
   * Get staff permissions
   * @param  mixed  id staff id
   * @return array
   */
  public List<StaffPermission> get_staff_permissions(int id)
  {
    // Fix for version 2.3.1 tables upgrade
    if (defined("DOING_DATABASE_UPGRADE")) return new List<StaffPermission>();

    var permissions = app_object_cache.get<List<StaffPermission>>($"staff-{id}-permissions");

    if (permissions != null) return permissions;
    permissions = db.StaffPermissions.Where(x => x.StaffId == id).ToList();
    app_object_cache.add($"staff-{id}-permissions", permissions);

    return permissions;
  }

  /**
   * Add new staff member
   * @param array data staff _POST data
   */
  public async Task<int> add(Staff data)
  {
    // First check for all cases if the email exists.
    data = hooks.apply_filters("before_create_staff_member", data);
    var email = db.Staff.Any(x => x.Email == data.Email);

    if (email) self.helper.die("Email already exists");

    data.IsAdmin = false || db.is_admin();

    var send_welcome_email = true;
    var original_password = data.Password;
    // if (!isset(data['send_welcome_email']))
    // {
    //   send_welcome_email = false;
    // }
    // else
    // {
    //   unset(data['send_welcome_email']);
    // }

    data.Password = self.HashPassword(data.Password);
    data.DateCreated = DateTime.Now;
    var departments = data.StaffDepartments.Any() ? data.StaffDepartments : new List<StaffDepartment>();
    // List<StaffPermission> permissions = new List<StaffPermission>();
    var permissions = new List<StaffPermission>();
    if (data.StaffPermissions.Any())
    {
      permissions = (List<StaffPermission>)data.StaffPermissions;
      data.StaffPermissions.Clear();
    }

    // var custom_fields = new Dictionary<string, Dictionary<int, string>>();
    var custom_fields = new List<CustomField>();
    // if (isset(data['custom_fields']))
    // {
    // var custom_fields = data['custom_fields'];
    //   unset(data['custom_fields']);
    // }

    if (data.IsAdmin) data.IsNotStaff = 0;

    db.Staff.Add(data);
    var staffid = data.Id;
    if (staffid <= 0) return 0;

    var slug = $"{data.FirstName} {data.LastName}";
    if (string.IsNullOrEmpty(slug)) slug = $"unknown-{staffid}";
    if (send_welcome_email) db.send_mail_template("staff_created", data.Email, staffid, original_password);
    var mediaPathSlug = self.helper.slug_it(slug);
    await db.Staff.Where(x => x.Id == staffid).UpdateAsync(x => new Staff { MediaPathSlug = mediaPathSlug });
    await db.SaveChangesAsync();

    if (custom_fields.Any()) self.helper.handle_custom_fields_post(staffid, custom_fields);
    if (departments.Any())
    {
      db.StaffDepartments.AddRange(departments.Select(department => new StaffDepartment { StaffId = staffid, DepartmentId = department.Id }).ToList());
      await db.SaveChangesAsync();
    }

    // Delete all staff permission if is admin we dont need permissions stored in database (in case admin check some permissions)
    await update_permissions(data.IsAdmin ? new List<StaffPermission>() : permissions, staffid);

    log_activity($"New Staff Member Added [ID: {staffid}, {data.FirstName} {data.LastName}]");
    // Get all announcements and set it to read.
    db.DismissedAnnouncements.AddRange(
      db.Announcements.Where(x => x.ShowToStaff).ToList()
        .Select(announcement =>
          new DismissedAnnouncement
          {
            AnnouncementId = announcement.Id,
            IsStaff = true,
            UserId = staffid
          }).ToList());
    db.SaveChanges();
    hooks.do_action("staff_member_created", staffid);
    return staffid;
  }

  /**
   * Update staff member info
   * @param  array data staff data
   * @param  mixed  id   staff id
   * @return boolean
   */
  public async Task<dynamic> update(Staff data, int id)
  {
    data = hooks.apply_filters("before_update_staff_member", data);

    if (db.is_admin())
    {
      if (data.IsAdmin)
      {
        data.IsAdmin = true;
        // unset(data.IsAdmin);
      }
      else
      {
        if (id != staff_user_id)
        {
          if (id == 1) return new { cant_remove_main_admin = true };
        }
        else
        {
          return new { cant_remove_yourself_from_admin = true };
        }

        data.IsAdmin = false;
      }
    }

    var affectedRows = 0;
    var departments = new List<StaffDepartment>();
    if (data.StaffDepartments.Any())
    {
      departments = (List<StaffDepartment>)data.StaffDepartments;
      data.StaffDepartments.Clear();
    }

    var permissions = new List<StaffPermission>();
    if (data.StaffPermissions.Any())
    {
      permissions = (List<StaffPermission>)data.StaffPermissions;
      data.StaffPermissions.Clear();
    }

    // if (isset(data['custom_fields']))
    // {
    //   var custom_fields = data['custom_fields'];
    //   if (handle_custom_fields_post(id, custom_fields)) affectedRows++;
    //   unset(data['custom_fields']);
    // }

    if (string.IsNullOrEmpty(data.Password))
    {
      data.Password = null;
    }
    else
    {
      data.Password = self.HashPassword(data.Password);
      data.LastPasswordChange = DateTime.Now;
    }


    // if (isset(data['two_factor_auth_enabled'])) {
    //     data['two_factor_auth_enabled'] = 1;
    // } else {
    //     data['two_factor_auth_enabled'] = 0;
    // }

    data.IsNotStaff = data.IsNotStaff == 0 ? 1 : 0;
    if (data.IsAdmin) data.IsNotStaff = 0;

    var staff_departments = departments_model.get_staff_departments(id);
    if (staff_departments.Any())
    {
      if (data.StaffDepartments.Any())
      {
        db.StaffDepartments.RemoveRange(db.StaffDepartments.Where(x => x.StaffId == id));
        await db.SaveChangesAsync();
      }
      else
      {
        staff_departments
          .Where(x => departments.Any())
          .Where(x => departments.All(x => x.Id != x.Id))
          .ToList()
          .ForEach(staff_department =>
          {
            var result = db.StaffDepartments
              .Where(x =>
                x.StaffId == id &&
                x.DepartmentId == staff_department.Id
              )
              .Delete();
            if (result > 0) affectedRows++;
          });
      }

      departments.ForEach(department =>
      {
        var _exists = db.StaffDepartments.Any(x => x.StaffId == id && x.DepartmentId == department.Id);
        if (!_exists) return;
        db.StaffDepartments
          .Add(new StaffDepartment
          {
            StaffId = id,
            DepartmentId = department.Id
          });
      });
    }
    else
    {
      if (departments.Any())
      {
        var departmentsDataset = departments.Select(department => new StaffDepartment
          {
            StaffId = id,
            DepartmentId = department.Id
          })
          .ToList();
        db.AddRange(departmentsDataset);
        affectedRows += await db.SaveChangesAsync();
      }
    }

    await db.Staff.Where(x => x.Id == id).UpdateAsync(x => data);
    var affected_rows = await db.SaveChangesAsync();
    if (affected_rows > 0) affectedRows++;

    if (await update_permissions(data.IsAdmin && data.IsAdmin ? new List<StaffPermission>() : permissions, id))
      affectedRows++;

    if (affectedRows <= 0) return false;
    hooks.do_action("staff_member_updated", id);
    log_activity($"Staff Member Updated [ID: {id}, {data.FirstName} {data.LastName}]");
    return true;
  }

  public async Task<bool> update_permissions(List<StaffPermission> permissions, int id)
  {
    var result = await db.StaffPermissions.Where(x => x.StaffId == id).DeleteAsync();
    var is_staff_member = db.is_staff_member(id);
    permissions.ForEach(sp =>
    {
      var feature = sp.Feature;
      var capabilities = sp.Capability;
      var items = JsonConvert.DeserializeObject<List<string>>(capabilities);
      if (items == null) return;
      items.ForEach(capability =>
      {
        // Maybe do this via hook.
        if (feature == "leads" && !is_staff_member) return;
        db.StaffPermissions.Add(new StaffPermission
        {
          StaffId = id,
          Feature = feature,
          Capability = capability
        });
      });
    });


    return true;
  }

  public bool update_profile(Staff data)
  {
    var id = data.Id;
    data = hooks.apply_filters("before_staff_update_profile", data);

    if (!string.IsNullOrEmpty(data.Password))
    {
      data.Password = null;
    }
    else
    {
      data.Password = self.HashPassword(data.Password);
      data.LastPasswordChange = DateTime.Now;
    }

    data.TwoFactorAuthEnabled = 0;
    data.GoogleAuthSecret = null;

    db.Staff.Where(x => x.Id == id).Update(x => data);
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    hooks.do_action("staff_member_profile_updated", id);
    log_activity($"Staff Profile Updated [Staff: {db.get_staff_full_name(id)}]");
    return true;
  }

  /**
   * Change staff passwordn
   * @param  mixed data   password data
   * @param  mixed userid staff id
   * @return mixed
   */
  public async Task<object> change_password(Staff data, int userid)
  {
    // data = hooks.apply_filters("before_staff_change_password", data, userid);
    data = hooks.apply_filters("before_staff_change_password", data);
    var member = get(x => x.Id == userid).First();
    // CHeck if member is active
    if (member.Active.Value)
      return new[]
      {
        new { memberinactive = true }
      };

    // Check new old password
    // if (!VerifyPassword(data['oldpassword'], member.Password))
    //   return new[]
    //   {
    //     new { passwordnotmatch = true }
    //   };

    data.Password = self.HashPassword(data.Password);
    var affected_rows = await db.Staff
      .Where(x => x.Id == userid)
      .UpdateAsync(x => new Staff { Password = data.Password, LastPasswordChange = DateTime.Now });
    if (affected_rows <= 0) return false;
    log_activity($"Staff Password Changed [{userid}]");
    return true;
  }

  /**
   * Change staff status / active / inactive
   * @param  mixed  id     staff id
   * @param  mixed status status(0/1)
   */
  public void change_staff_status(int id, int status)
  {
    status = hooks.apply_filters("before_staff_status_change", status);
    db.Staff.Where(x => x.Id == id).Update(x => new Staff { Active = status == 1 });
    db.SaveChanges();
    log_activity($"Staff Status Changed [StaffID: {id} - Status(Active/Inactive): {status}]");
  }

  public async Task<LoggedTimeData> get_logged_time_data(int? staffId = null, Dictionary<string, string> filterData = null)
  {
    staffId ??= staff_user_id;
    var result = new LoggedTime();
    var now = DateTime.UtcNow;

    // Define date ranges
    var firstDayThisMonth = self.helper.first_day_of_this_month();
    var lastDayThisMonth = self.helper.last_day_of_this_month();

    var firstDayLastMonth = self.helper.first_day_of_this_week();
    var lastDayLastMonth = self.helper.last_day_of_previous_month();

    var firstDayThisWeek = self.helper.first_day_of_this_week();
    var lastDayThisWeek = self.helper.last_day_of_this_week();

    var firstDayLastWeek = self.helper.first_day_of_previous_week();
    var lastDayLastWeek = self.helper.last_day_of_previous_week();

    // Query the timers
    var timers = db.TasksTimers
      .Include(x => x.Task)
      .Where(t => t.StaffId == staffId)
      .ToList();

    var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var filterPeriod = false;
    DateTime from = DateTime.MinValue, to = DateTime.MinValue;

    if (filterData != null && filterData.ContainsKey("period-from") && filterData.ContainsKey("period-to"))
    {
      filterPeriod = true;
      from = DateTime.Parse(filterData["period-from"]);
      to = DateTime.Parse(filterData["period-to"]);
    }

    foreach (var timer in timers)
    {
      // var startTime = DateTimeOffset.FromUnixTimeSeconds(timer.StartTime).DateTime;
      var startTime = timer.StartTime;
      var endTime = !timer.EndTime.HasValue ? DateTime.Now : timer.EndTime;
      var notFinished = timer.EndTime == null;

      var total = endTime - startTime;

      // Accumulate totals

      result.total.Add(total.Value.Ticks);

      if (startTime >= firstDayThisMonth && startTime <= lastDayThisMonth)
      {
        result.this_month.Add(total.Value.Ticks);
        if (filterData != null && filterData.ContainsKey("this_month"))
          result.timesheets[timer.Id] = timer;
      }

      if (startTime >= firstDayLastMonth && startTime <= lastDayLastMonth)
      {
        result.last_month.Add(total.Value.Ticks);
        if (filterData != null && filterData.ContainsKey("last_month"))
          result.timesheets[timer.Id] = timer;
      }

      if (startTime >= firstDayThisWeek && startTime <= lastDayThisWeek)
      {
        result.this_week.Add(total.Value.Ticks);
        if (filterData != null && filterData.ContainsKey("this_week"))
          result.timesheets[timer.Id] = timer;
      }

      if (startTime >= firstDayLastWeek && startTime <= lastDayLastWeek)
      {
        result.last_week.Add(total.Value.Ticks);
        if (filterData != null && filterData.ContainsKey("last_week"))
          result.timesheets[timer.Id] = timer;
      }

      if (filterPeriod && startTime >= from && startTime <= to)
        result.timesheets[timer.Id] = timer;
    }

    // Sum the totals
    var output = new LoggedTimeData
    {
      total = result.total.Sum(),
      this_month = result.this_month.Sum(),
      last_month = result.last_month.Sum(),
      this_week = result.this_week.Sum(),
      last_week = result.last_week.Sum()
    };

    return output;
  }
}
