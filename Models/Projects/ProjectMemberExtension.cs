using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Task = System.Threading.Tasks.Task;

namespace Service.Models.Projects;

public static class ProjectMemberExtension
{
  public static async Task delete(this ProjectsModel model, int project_id)
  {
    var (self, db) = model.getInstance();
    hooks.do_action("before_project_deleted", project_id);
    var project_name = db.get_project_name_by_id(project_id);
    var query = db.Projects.Where(x => x.Id == project_id).AsQueryable();


    await db.ProjectMembers.Where(x => x.ProjectId == project_id).DeleteAsync();
    await db.ProjectNotes.Where(x => x.ProjectId == project_id).DeleteAsync();
    await db.Milestones.Where(x => x.ProjectId == project_id).DeleteAsync();

    // Delete the custom field values
    await db.CustomFieldsValues.Where(x => x.RelId == project_id && x.FieldTo == "projects").DeleteAsync();
    await db.Taggables.Where(x => x.RelId == project_id && x.RelType == "project").DeleteAsync();
    var discussions = db.ProjectDiscussions.Where(x => x.ProjectId == project_id).ToList();
    discussions.ForEach(discussion =>
    {
      var discussion_comments = model.get_discussion_comments(discussion.Id, "regular");
      foreach (var comment in discussion_comments) model.delete_discussion_comment_attachment(comment.Data?.FileName, discussion.Id);
      db.ProjectDiscussionComments.Where(x => x.DiscussionId == discussion.Id).Delete();
    });

    await db.ProjectDiscussions.Where(x => x.ProjectId == project_id).DeleteAsync();

    var files = model.get_files(project_id);
    files.ForEach(file =>
    {
      var path = Path.Combine(Directory.GetCurrentDirectory(), file.Id.ToString());
      file_delete(path);
    });
    var tasks_model = self.tasks_model(db);

    var tasks = await model.get_tasks(x => x.Id == project_id);
    tasks.ForEach(task => { tasks_model.delete_task(task.Id, false); });

    await db.ProjectSettings
      .Where(x => x.ProjectId == project_id)
      .DeleteAsync();
    await db.ProjectActivities
      .Where(x => x.ProjectId == project_id)
      .DeleteAsync();
    await db.Expenses
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new Expense
      {
        ProjectId = 0
      });
    await db.Contracts
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new Contract
      {
        ProjectId = 0
      });
    await db.Invoices
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new Invoice
      {
        ProjectId = 0
      });
    await db.CreditNotes
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new CreditNote
      {
        ProjectId = 0
      });
    await db.Estimates
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new Estimate
      {
        ProjectId = 0
      });

    await db.Tickets
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new Ticket
      {
        ProjectId = 0
      });

    await db.Proposals
      .Where(x => x.ProjectId == project_id)
      .UpdateAsync(x => new Proposal
      {
        ProjectId = 0
      });
    await db.PinnedProjects.Where(x => x.ProjectId == project_id).DeleteAsync();
    log_activity($"Project Deleted [ID: {project_id}, Name: {project_name}]");
    hooks.do_action("after_project_deleted", project_id);
  }

  public static List<DataSet<ProjectActivity>> get_activity(this ProjectsModel model, object id, int limit = 0, bool only_project_members_activity = false)
  {
    var (self, db) = model.getInstance();
    var query = db.ProjectActivities.Where(x => x.ProjectId == Convert.ToInt32(id)).AsQueryable();
    if (!db.client_logged_in())
    {
      var can_view_project = db.has_permission("projects", "", "view");
      if (!can_view_project)
        query.Where(x =>
          db.ProjectMembers
            .Where(y => y.StaffId == model.staff_user_id)
            .Select(y => y.ProjectId)
            .ToList()
            .Contains(x.ProjectId)
        );
    }

    if (db.client_logged_in()) query.Where(x => x.VisibleToCustomer);
    if (int.TryParse($"{id}", out var project_id))
      query.Where(x => x.ProjectId == project_id);
    if (int.TryParse($"{limit}", out var _limit) && _limit > 0)
      query.Take(limit);

    var activities = query
      .OrderByDescending(x => x.DateCreated)
      .Include(projectActivity => projectActivity.Project)
      .ToList()
      .Select(x => new DataSet<ProjectActivity>() { Data = x })
      .Select(activity =>
      {
        var seconds = get_string_between(activity.Data.AdditionalData, "<seconds>", "</seconds>");
        var other_lang_keys = get_string_between(activity.Data.AdditionalData, "<lang>", "</lang>");
        var _additional_data = activity.Data.AdditionalData;
        if (seconds != "")
          _additional_data = $"<seconds>{seconds}</seconds>".Replace(seconds_to_time_format(Convert.ToInt32(seconds)), _additional_data);
        if (other_lang_keys != "") _additional_data = $"<lang>{other_lang_keys}</lang>".Replace(helper.label(other_lang_keys), _additional_data);
        if (_additional_data.Contains("project_status_"))
        {
          var row = model.get_project_status_by_id(Convert.ToInt32(strafter(_additional_data, "project_status_")));
          if (string.IsNullOrEmpty(row.Name))
            _additional_data = row.Name;
        }

        activity.Data.DescriptionKey = helper.label(activity.Data.DescriptionKey);
        activity.Data.AdditionalData = _additional_data;
        activity.Data.Project.Name = db.get_project_name_by_id(activity.Data.Id);
        activity.Data.DescriptionKey = null;
        return activity;
      })
      .ToList();
    return activities;
  }


  public static bool new_project_file_notification(this ProjectsModel model, int file_id, int project_id)
  {
    var (self, db) = model.getInstance();
    var file = model.get_file(file_id);

    var additional_data = file.FileName;
    log_activity(project_id, "project_activity_uploaded_file", additional_data, file.VisibleToCustomer);
    var notification_data = new Notification
    {
      Description = "not_project_file_uploaded",
      Link = $"projects/view/{project_id}?group=project_files&file_id={file_id}"
    };


    if (db.client_logged_in())
      notification_data.FromClientId = db.get_contact_user_id();
    else
      notification_data.FromUserId = model.staff_user_id;


    var members = model.get_project_members(project_id);
    var notifiedUsers = members
      .Select(member =>
      {
        if (member.StaffId == model.staff_user_id && !db.client_logged_in()) return 0;
        notification_data.ToUserId = member.StaffId;
        return db.add_notification(notification_data)
          ? member.StaffId
          : 0;
      })
      .ToList();

    db.pusher_trigger_notification(notifiedUsers);

    return model.send_project_email_template(
      project_id,
      "project_file_to_staff",
      "project_file_to_customer",
      file.VisibleToCustomer,
      new
      {
        staff = new
        {
          DiscussionId = file_id,
          DiscussionType = "file"
        },
        customers = new
        {
          CustomerTemplate = true,
          DiscussionId = file_id,
          DiscussionType = "file"
        }
      }
    );
  }

  public static int add_external_file(this ProjectsModel model, ProjectFile data)
  {
    var (self, db) = model.getInstance();
    var file = db.get_project_files(data.Id).FirstOrDefault();
    var insert = new ProjectFile();
    insert.DateCreated = DateTime.Now;
    insert.Id = data.Id;
    insert.External = data.External;
    insert.VisibleToCustomer = data.VisibleToCustomer;
    insert.FileName = file?.FileName;
    insert.Subject = file?.FileName;
    insert.ExternalLink = file?.ExternalLink;

    insert.FileType = self.get_mime_by_extension(file.FileName);
    if (!string.IsNullOrEmpty(file.ThumbnailLink))
      insert.ThumbnailLink = file.ThumbnailLink;
    if (data.StaffId > 0)
      insert.StaffId = data.StaffId;
    else if (data.ContactId > 0) insert.ContactId = data.ContactId;
    var result = db.ProjectFiles.Add(insert);
    if (!result.IsAdded()) return 0;
    model.new_project_file_notification(result.Entity.Id, data.Id);
    return result.Entity.Id;
  }

  public static bool send_project_email_template(this ProjectsModel model, int project_id, string staff_template, string customer_template, bool action_visible_to_customer, dynamic additional_data = default)
  {
    var (self, db) = model.getInstance();
    if (additional_data != null)
    {
      additional_data.Customers = new List<object>();
      additional_data.staff = new List<object>();
    }
    else if (additional_data.Count() == 1)
    {
      if (!additional_data.IsStaff)
        additional_data.Staff = new List<Staff>();
      else
        additional_data.customers = new List<Contact>();
    }

    var project = model.get(x => x.Id == project_id);
    model.get_project_members(project_id)
      .ForEach(member =>
      {
        if (db.is_staff_logged_in() && member.StaffId == model.staff_user_id) return;
        var mailTemplate = mail_template(staff_template, project, member, additional_data.staff);
        if (additional_data.Attachments.Any())
          foreach (var attachment in additional_data.Attachments)
            mailTemplate.add_attachment(attachment);
        mailTemplate.send();
      });

    if (action_visible_to_customer != true) return true;
    var clients_model = self.clients_model(db);
    var contacts = clients_model.get_contacts_for_project_notifications(project_id, "project_emails");
    contacts
      .Where(contact => !db.client_logged_in() || contact.Id != db.get_contact_user_id())
      .Select(contact =>
      {
        var mailTemplate = mail_template(customer_template, project, contact, additional_data.customers);
        if (additional_data.Attachments)
          foreach (var attachment in additional_data.Attachments)
            mailTemplate.add_attachment(attachment);
        mailTemplate.send();
        return true;
      })
      .ToList();


    return true;
  }

  public static List<Project> get_project_billing_data(this ProjectsModel model, int id)
  {
    var (self, db) = model.getInstance();
    var rows = db.Projects.Where(x => x.Id == id).ToList();
    return rows;
  }

  public static async Task<(string hours, double total_money)> total_logged_time_by_billing_type(this ProjectsModel model, int id, Expression<Func<bool>> conditions)
  {
    var (self, db) = model.getInstance();
    var projects_model = self.projects_model(db);
    var project_data = model.get_project_billing_data(id).FirstOrDefault();
    var data = new List<object>();
    var logged_time = string.Empty;
    var total_money = 0.0;
    if (project_data.BillingType == 2)
    {
      var seconds = model.total_logged_time(id);
      var temp = projects_model.calculate_total_by_project_hourly_rate(seconds, project_data.ProjectRatePerHour);
      logged_time = temp.hours;
      total_money = temp.total_money;
    }
    else if (project_data.BillingType == 3)
    {
      var temp = await model.get_data_total_logged_time(x => x.Id == id);
      logged_time = temp.logged_time;
      total_money = temp.total_money;
    }

    return (logged_time, total_money);
  }

  public static async Task<bool> data_billable_time(this ProjectsModel model, int id)
  {
    var output = await model.get_data_total_logged_time(x => x.Id == id && x.Billable == 1);
    return output is { total_money: > 0, total_seconds: > 0 };
  }

  public static async Task<bool> data_billed_time(this ProjectsModel model, int id)
  {
    var output = await model.get_data_total_logged_time(x => x.Id == id && x.Billable == 1 && x.Billed == 1);
    return output is { total_money: > 0, total_seconds: > 0 };
  }

  public static async Task<bool> data_unbilled_time(this ProjectsModel model, int id)
  {
    var output = await model.get_data_total_logged_time(x => x.Id == id && x.Billable == 1 && x.Billed == 0);
    return output is { total_money: > 0, total_seconds: > 0 };
  }

  public static bool delete_discussion_comments(this ProjectsModel model, int id, string type)
  {
    var (self, db) = model.getInstance();
    var comments = db.ProjectDiscussionComments.Where(x => x.DiscussionId == id && x.DiscussionType == type).ToList();
    foreach (var comment in comments) model.delete_discussion_comment_attachment(comment.FileName, id);
    db.ProjectDiscussionComments.Where(x => x.DiscussionId == id && x.DiscussionType == type).Delete();
    return false;
  }

  public static async Task<(double total_money, double total_seconds, string logged_time)> get_data_total_logged_time(this ProjectsModel model, Expression<Func<Entities.Task, bool>> conditions)
  {
    var (self, db) = model.getInstance();
    (double total_money, int total_seconds, string logged_time) data = new();
    var id = db.ExtractIdFromCondition(conditions) ?? 0;
    var project_data = model.get_project_billing_data(id).FirstOrDefault();
    var tasks = await model.get_tasks(conditions.And(x => x.Id == id));
    if (project_data.BillingType == 3)
    {
      var (total_money, total_seconds) = model.calculate_total_by_task_hourly_rate(tasks.ToArray());
      var logged_time = seconds_to_time_format(total_seconds);
      return (total_money, total_seconds, logged_time);
    }

    if (project_data.BillingType == 2)
    {
      var seconds = tasks.Aggregate(0, (current, task) => current + total_logged_time(task));
      var row = model.calculate_total_by_project_hourly_rate(seconds, project_data.ProjectRatePerHour);
      var logged_time = row.hours;
      return (0, 0, logged_time);
    }

    return data;
  }

  public static bool update_discussion_last_activity(this ProjectsModel model, int id, string type)
  {
    var (self, db) = model.getInstance();
    if (type == "file")
      db.ProjectFiles.Where(x => x.Id == id).Update(x => new ProjectFile
      {
        LastActivity = date("Y-m-d H:i:s")
      });
    else
      db.ProjectDiscussions
        .Where(x => x.Id == id)
        .Update(x => new ProjectDiscussion
        {
          LastActivity = DateTime.Now
        });

    return true;
  }

  public static async Task send_project_email_mentioned_users(this ProjectsModel model, int project_id, string staff_template, List<Staff> staff, object additional_data)
  {
    var (self, db) = model.getInstance();
    var project = model.get(x => x.Id == project_id);
    var staff_model = self.staff_model(db);
    staff
      .Select(x => x.Id)
      .Where(staffId => !db.is_staff_logged_in() || staffId != model.staff_user_id)
      .Select(staffId => staff_model.get(x => x.Id == staffId).FirstOrDefault())
      // .Select(member =>  helper.mail_template(staff_template, project, member, additional_data.Staff))
      .Select(member => mail_template(staff_template, project, member, convert<Staff>(additional_data)))
      .ToList()
      .ForEach(mailTemplate =>
      {
        var attachments = db.get_project_files(Convert.ToInt32(additional_data));
        if (attachments.Any())
          foreach (var attachment in attachments)
            mailTemplate.add_attachment(attachment);
        mailTemplate.send();
      });
  }

  public static bool convert_estimate_items_to_tasks(this ProjectsModel model, int project_id, List<Itemable> items, List<int> assignees, DataSet<Project> project_data, List<string> project_settings)
  {
    var (self, db) = model.getInstance();
    var staff_model = self.staff_model(db);
    var tasks_model = self.tasks_model(db);
    for (var index = 0; index < items.Count; index++)
    {
      var itemId = items[index];
      var _item =
        db.Itemables.FirstOrDefault(x => x.Id == itemId.Id);

      var dataset = new DataSet<Entities.Task>();
      dataset.Data = new Entities.Task
      {
        Billable = 1,
        Name = _item.Description,
        Description = _item.LongDescription,
        StartDate = project_data.Data.StartDate!.Value,
        DueDate = null,
        RelType = "project",
        RelId = project_id,
        HourlyRate = project_data.Data.BillingType == 3 ? _item.Rate : 0,
        Priority = db.get_option<int>("default_task_priority")
      };
      dataset["with_default_assignee"] = false;

      if (db.view_tasks(project_settings).Any())
        dataset.Data.VisibleToClient = true;
      var task_id = tasks_model.add(dataset);
      if (task_id == 0) return false;
      var staff_id = assignees[index];

      tasks_model.add_task_assignees(new TaskAssigned
      {
        TaskId = task_id,
        AssignedFrom = staff_id
      });

      if (!model.is_member(project_id, staff_id))
        db.ProjectMembers.Add(new ProjectMember
        {
          ProjectId = project_id,
          StaffId = staff_id
        });
    }

    return false;
  }

  /**
   * @deprecated
   *
   * @param  int id
   * @param  string type
   *
   * @return array
   */
  public static bool get_project_overview_weekly_chart_data(this ProjectsModel model, int id, string type = "this_week")
  {
    //_deprecated_function("Projects_model::get_project_overview_weekly_chart_data", "2.9.2", "HoursOverviewChart class");
    // return new HoursOverviewChart(id, type).get();
    return false;
  }

  /**
   * @deprecated
   *
   * @param  array filters
   *
   * @return array
   */
  public static bool get_all_projects_gantt_data(params string[] filters)
  {
    //_deprecated_function("Projects_model::get_all_projects_gantt_data", "2.9.2", "AllProjectsGantt class");
    // return new AllProjectsGantt(filters).get();
    return false;
  }

  /**
   * @deprecated
   *
   * @return array
   */
  public static bool get_gantt_data(this ProjectsModel model, int project_id, string type = "milestones", int? taskStatus = null, Expression<Func<object, bool>> type_where = null)
  {
    //_deprecated_function("Projects_model::get_gantt_data", "2.9.2", "Gantt class");

    // return new Gantt(project_id, type).forTaskStatus(taskStatus)
    //   .excludeMilestonesFromCustomer(type_where.HideFromCustomer && type_where.HideFromCustomer == 1)
    //   .get();
    return false;
  }
}
