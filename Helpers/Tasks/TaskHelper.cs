using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Service.Core.Extensions;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers.Entities;
using Service.Helpers.Proposals;
using Service.Models.Tasks;
using Task = Service.Entities.Task;

namespace Service.Helpers.Tasks;

using static InvoiceHelper;
using static StaffHelper;

public static class TaskHelper
{
  // Function that formats task status for the final user
  public static string format_task_status(this HelperBase helper, DataSet<Task> status = null, bool text = false, bool clean = false)
  {
    var (self, db) = getInstance();
    status ??= helper.get_task_status_by_id(status.Data?.Id ?? 0);
    // var statusName = hooks.apply_filters("task_status_name", status.Name, status);
    var statusName = hooks.apply_filters("task_status_name", status.Data.Name);

    if (clean) return statusName;

    var style = string.Empty;
    var cssClass = string.Empty;
    var color = "#333";
    if (!text)
    {
      color = status.Options.FirstOrDefault(x => x.Name == "color")?.Value ?? "#333";
      style = $"color:{color};border:1px solid {helper.adjust_hex_brightness(color, 0.4)};background: {helper.adjust_hex_brightness(color, 0.04)};";
      cssClass = "label";
    }
    else
    {
      style = $"color:{color};";
    }

    return $"<span class='{cssClass}' style='{style}'>{statusName}</span>";
  }

  // Return predefined tasks priorities
  public static List<TaskPriority> GetTasksPriorities(this HelperBase helper)
  {
    var (self, db) = getInstance();
    return hooks.apply_filters("tasks_priorities", new List<TaskPriority>
    {
      new() { Id = 1, Name = "Low", Color = "#777" },
      new() { Id = 2, Name = "Medium", Color = "#03a9f4" },
      new() { Id = 3, Name = "High", Color = "#ff6f00" },
      new() { Id = 4, Name = "Urgent", Color = "#fc2d42" }
    });
  }

  // Get project name by passed id
  public static string get_task_subject_by_dd(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var task = db.Tasks.FirstOrDefault(t => t.Id == id);
    return task?.Name ?? string.Empty;
  }

  // Get task status by passed task id
  // public static TaskOption? get_task_status_by_id(this HelperBase helper, int id)
  public static DataSet<Task>? get_task_status_by_id(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var tasks_model = self.tasks_model(db);

    var statuses = tasks_model.get_statuses();

    var output = new DataSet<Task>();


    var status = new TaskOption
    {
      id = 0,
      bg_color = "#333",
      text_color = "#333",
      name = "[Status Not Found]",
      order = 1
    };
    var temp = statuses.Where(x => x.id == id).FirstOrDefault();
    if (temp == null) status = temp;

    // foreach ($statuses as $s) {
    //   if ($s['id'] == $id) {
    //     $status = $s;
    //     break;
    //   }
    // }
    return output;
  }

  // Format task priority based on passed priority id
  public static string TaskPriority(this HelperBase helper, int id)
  {
    var priority = helper.GetTasksPriorities().FirstOrDefault(p => p.Id == id);
    return priority?.Name ?? id.ToString();
  }

  // Get and return task priority color
  public static string TaskPriorityColor(this HelperBase helper, int id)
  {
    var priority = helper.GetTasksPriorities().FirstOrDefault(p => p.Id == id);
    return priority?.Color ?? "#333";
  }

  // Format HTML task assignees
  public static string FormatMembersByIdsAndNames(this HelperBase helper, string ids, string names, string size = "md")
  {
    if (string.IsNullOrEmpty(ids)) return string.Empty;

    var assignees = names.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
    var assigneeIds = ids.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
    var outputAssignees = "<div class=\"tw-flex -tw-space-x-1\">";
    var exportAssignees = string.Empty;
    assigneeIds.ForEach(assigneeId =>
    {
      assignees.ForEach(assigned =>
      {
        if (assigned == 0) return;
        outputAssignees += $"<a href=\"/profile/{assigneeId}\">{helper.staff_profile_image(assigneeId, size)}</a>";
        exportAssignees += assigned + ", ";
      });
    });


    if (!string.IsNullOrEmpty(exportAssignees)) outputAssignees += $"<span class=\"hide\">{exportAssignees.Substring(0, exportAssignees.Length - 2)}</span>";

    outputAssignees += "</div>";

    return outputAssignees;
  }

  // Format task relation name
  public static string TaskRelName(this HelperBase helper, string relName, int relId, string relType)
  {
    return relType switch
    {
      "invoice" => helper.format_invoice_number(relId),
      "estimate" => helper.format_estimate_number(relId),
      "proposal" => helper.format_proposal_number(relId),
      _ => relName
    };
  }

  // Task relation link


  public static string task_rel_link(this NavigationManager nav, int relId, string relType)
  {
    var linkMappings = new Dictionary<string, Func<object, string>>
    {
      { "customer", id => nav.admin_url($"clients/client/{id}") },
      { "invoice", id => nav.admin_url($"invoices/list_invoices/{id}") },
      { "project", id => nav.admin_url($"projects/view/{id}") },
      { "estimate", id => nav.admin_url($"estimates/list_estimates/{id}") },
      { "contract", id => nav.admin_url($"contracts/contract/{id}") },
      { "ticket", id => nav.admin_url($"tickets/ticket/{id}") },
      { "expense", id => nav.admin_url($"expenses/list_expenses/{id}") },
      { "lead", id => nav.admin_url($"leads/index/{id}") },
      { "proposal", id => nav.admin_url($"proposals/list_proposals/{id}") }
    };

    var lowerRelType = relType.ToLower();

    return linkMappings.TryGetValue(lowerRelType, out var linkFunc) ? linkFunc(relId) : "#";
  }

  /**
 * Get project name by passed id
 * @param  mixed $id
 * @return string
 */
  public static string get_task_subject_by_id(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var task = db.Tasks
      .FirstOrDefault(x => x.Id == id);
    return task == null ? task.Name : string.Empty;
  }

  /**
 * This text is used in WHERE statements for tasks if the staff member don't have permission for tasks VIEW
 * This query will shown only tasks that are created from current user, public tasks or where this user is added is task follower.
 * Other statement will be included the tasks to be visible for this user only if Show All Tasks For Project Members is set to YES
 * @return string
 */
  public static Expression<Func<Task, bool>> get_tasks_where_string(this HelperBase helper, bool table = true)
  {
    var (self, db) = getInstance();
    var staffUserId = db.get_staff_user_id(); // Assuming a method to get the staff user ID like get_staff_user_id()
    var showAllTasksForProjectMember = db.get_option_compare("show_all_tasks_for_project_member", 1); // Assuming a GetOption method

    // Building the expression tree to match the conditions
    Expression<Func<Task, bool>> filter = t =>
      // Tasks assigned to the current staff user
      db.Tasks.Any(t => db.Staff
        .Include(x => x.TaskAssigneds)
        .Any(a => a.Id == staffUserId && a.TaskAssigneds.Any(ta => ta.TaskId == t.Id)))
      // Tasks where the current user is a follower
      || db.Tasks.Any(t => db.TaskFollowers.Any(f => f.StaffId == staffUserId))
      // Tasks added by the current user
      || (t.AddedFrom == staffUserId && !t.IsAddedFromContact)
      // If the "show all tasks for project member" option is enabled
      || (showAllTasksForProjectMember && db.Tasks.Any(t =>
        t.RelType == "project" && t.RelId.HasValue && t.RelId.Value == db.ProjectMembers.FirstOrDefault(p => p.StaffId == staffUserId)!.ProjectId))
      // Public tasks
      || db.Tasks.Any(t => t.IsPublic);

    return filter;
  }

  /**
   * Round the given logged seconds of a task
   *
   * @since 2.7.1
   *
   * @param  int  seconds
   *
   * @return int
   */
  public static double task_timer_round(this HelperBase helper, int seconds)
  {
    var (self, db) = getInstance();
    var roundMinutes = db.get_option<int>("round_off_task_timer_time");
    var roundSeconds = roundMinutes * 60;
    return db.get_option<int>("round_off_task_timer_option") switch
    {
      1 => // up
        Math.Ceiling((double)(seconds / roundSeconds)) * roundSeconds,
      2 => // down
        Math.Floor((double)(seconds / roundSeconds)) * roundSeconds,
      3 => // nearest
        Math.Round((double)(seconds / roundSeconds)) * roundSeconds,
      _ => seconds
    };
  }

  public static double total_logged_time(this Task task)
  {
    return 0;
  }
}
