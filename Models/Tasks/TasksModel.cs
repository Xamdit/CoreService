using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Service.Helpers.Relations;
using Service.Helpers.Sale;
using Service.Helpers.Tags;
using Service.Helpers.Tasks;
using Service.Models.Client;
using Service.Models.Leads;
using Service.Models.Misc;
using Service.Models.Projects;
using Service.Models.Users;
using static Service.Models.Tasks.TaskStatus;
using static Service.Framework.Core.Extensions.StringExtension;
using File = Service.Entities.File;
using Task = Service.Entities.Task;


namespace Service.Models.Tasks;

public class TasksModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private List<TaskOption> statuses = new();
  private LeadsModel leads_model = self.leads_model(db);
  private StaffModel staff_model = self.staff_model(db);
  private ProjectsModel projects_model = self.projects_model(db);
  private ClientsModel clients_model = self.clients_model(db);
  private MiscModel misc_model = self.misc_model(db);

  // Not used?
  public List<Task> get_user_tasks_assigned()
  {
    var rows = db.Tasks
      .Where(x =>
        db.TaskAssigneds.Any(y => y.StaffId == staff_user_id) &&
        x.Status != 5)
      .OrderBy(x => x.DueDate)
      .ToList();
    return rows;
  }

  public IEnumerable<TaskOption> get_statuses()
  {
    statuses = new List<TaskOption>
    {
      new()
      {
        id = STATUS_NOT_STARTED,
        color = "#64748b",
        name = self.helper.label("task_status_1"),
        order = 1,
        filter_default = true
      },
      new()
      {
        id = STATUS_IN_PROGRESS,
        color = "#3b82f6",
        name = self.helper.label("task_status_4"),
        order = 2,
        filter_default = true
      },
      new()
      {
        id = STATUS_TESTING,
        color = "#0284c7",
        name = self.helper.label("task_status_3"),
        order = 3,
        filter_default = true
      },
      new()
      {
        id = STATUS_AWAITING_FEEDBACK,
        color = "#84cc16",
        name = self.helper.label("task_status_2"),
        order = 4,
        filter_default = true
      },
      new()
      {
        id = STATUS_COMPLETE,
        color = "#22c55e",
        name = self.helper.label("task_status_5"),
        order = 100,
        filter_default = false
      }
    };
    statuses = hooks.apply_filters("before_get_task_statuses", statuses);
    statuses = statuses.Aggregate(statuses, (a, b) => a.OrderBy(x => x.order).ToList());
    return statuses;
  }

  /**
   * Get task by id
   * @param  mixed id task id
   * @return object
   */
  public Task get(Expression<Func<Task, bool>> condition)
  {
    var id = db.ExtractIdFromCondition(condition) ?? 0;
    var task = new DataSet<Task>();
    condition = condition.And(x => x.Id == id);
    task.Data = db.Tasks.FirstOrDefault(condition);
    if (task == null)
      return hooks.apply_filters("get_task", task);
    task.Data.TaskComments = get_task_comments(id);
    task.Data.TaskAssigneds = get_task_assignees(id);
    task["assignees_ids"] = convert<List<TaskAssigned>>(task["task_assigneds"]).Select(x => x.Id).ToList();
    task["followers"] = get_task_followers(id);
    task["followers_ids"] = convert<List<TaskFollower>>(task["task_followers"]).Select(x => x.Id).ToList();

    task["attachments"] = get_task_attachments(x => x.Id == id);
    task["timesheets"] = get_timesheets(task.Data.Id);
    task["task_checklist_items"] = get_checklist_items(id);

    if (db.is_staff_logged_in())
    {
      task["current_user_is_assigned"] = is_task_assignee(staff_user_id, id);
      task["current_user_is_creator"] = is_task_creator(staff_user_id, id);
    }

// task.milestone_name = string.Empty;
    if (task.Data.RelType != "project") return hooks.apply_filters("get_task", task);
    task["project_data"] = projects_model.get(x => x.Id == task.Data.RelId);
    if (!task.Data.Milestone.HasValue) return hooks.apply_filters("get_task", task);
    var milestone = get_milestone(task.Data.Milestone.Value);
    if (milestone == null) return hooks.apply_filters("get_task", task);
    task["hide_milestone_from_customer"] = milestone.HideFromCustomer;
// task.Milestone.name = milestone.name;

    return hooks.apply_filters("get_task", task);
  }

  public Milestone? get_milestone(int id)
  {
    var row = db.Milestones.FirstOrDefault(x => x.Id == id);
    return row;
  }

  public void update_order(object data)
  {
    //AbstractKanban::updateOrder(data.order, 'kanban_order', 'tasks', data.Status);
  }

  public void get_distinct_tasks_years(object get_from)
  {
    // return db.query('SELECT DISTINCT(YEAR('+db.escape_str(get_from)+')) as year FROM  tasks WHERE '+db.escape_str(get_from)+' IS NOT NULL ORDER BY year DESC').result_array();
  }

  public bool is_task_billed(int id)
  {
    var output = db.Tasks.Any(x => x.Id == id && x.Billed == 1);
    return output;
  }

  public Task? copy(DataSet<Task> dataset, Task overwrites = default, Task copy_from = default)
  {
    var task = get(x => x.Id == copy_from.Id);
    var _new_task_data = new Task();
    _new_task_data = JsonConvert.DeserializeObject<Task>(JsonConvert.SerializeObject(task));
    _new_task_data.Status = (int)dataset["copy_task_status"] > 0 ? (int)dataset["copy_task_status"] : 1;
    _new_task_data.DateCreated = DateTime.Now;
    _new_task_data.StartDate = DateTime.Now;
    _new_task_data.DeadlineNotified = 0;
    _new_task_data.Billed = 0;
    _new_task_data.Billed = 0;
    _new_task_data.TotalCycles = 0;
    _new_task_data.IsRecurringFrom = null;

    if (db.is_staff_logged_in())
      _new_task_data.AddedFrom = staff_user_id;
    var dStart = task.StartDate;
    var dEnd = task.DueDate; // Assuming DueDate is DateTime?

    TimeSpan? dDiff = null;

    if (dEnd.HasValue) // Checking if DueDate is not null
    {
      dDiff = dEnd.Value - dStart; // Calculate the difference between start and end dates

      // Assuming you want to add the difference (in days) to the current date
      _new_task_data.DueDate = DateTime.Now.AddDays(dDiff.Value.Days);
    }
    else
    {
      // Handle case where DueDate is null
      // You might want to set a default value or handle this accordingly
      _new_task_data.DueDate = DateTime.Now;
    }

    // Overwrite data options
    // if (overwrites != default)
    //   foreach (var kvp in overwrites)
    //     // var key => val
    //     //   _new_task_data[key] = val;
    //     Console.WriteLine("TasksModel.copy");

    _new_task_data.DateFinished = null;
    _new_task_data = hooks.apply_filters("before_add_task", _new_task_data);
    var result = db.Tasks.Add(_new_task_data);
    var insert_id = result.Entity.Id;
    if (!result.IsAdded()) return null;
    var tags = db.get_tags_in(copy_from.RelId.Value, "task");
    db.handle_tags_save(tags, insert_id, "task");
    if (Convert.ToBoolean(dataset["copy_task_assignees"])) copy_task_assignees(copy_from, db.task(insert_id));
    if (Convert.ToBoolean(dataset["copy_task_followers"])) copy_task_followers(copy_from, db.task(insert_id));
    if (Convert.ToBoolean(dataset["copy_task_checklist_items"])) copy_task_checklist_items(copy_from, db.task(insert_id));
    if (Convert.ToBoolean(dataset["copy_task_attachments"]))
    {
      var attachments = get_task_attachments(x => x.Id == copy_from.Id);
      if (is_dir(get_upload_path_by_type("task") + copy_from))
        xcopy(get_upload_path_by_type("task") + copy_from, get_upload_path_by_type("task") + insert_id);
      var _at = attachments;
      var at = _at.Select(x =>
        {
          var file = new File();
          var external = string.Empty;
          if (string.IsNullOrEmpty(x.External)) return add_attachment_to_database(insert_id, x, external, false);
          external = x.External;
          file.FileName = x.FileName;
          file.ExternalLink = x.ExternalLink;
          if (!string.IsNullOrEmpty(x.ThumbnailLink))
            file.ThumbnailLink = x.ThumbnailLink;
          return add_attachment_to_database(insert_id, x, external, false);
        })
        .ToList();
    }

    copy_task_custom_fields(copy_from, db.task(insert_id));
    hooks.do_action("after_add_task", insert_id);
    return db.task(insert_id);
  }

  public void copy_task_followers(Task from_task, Task to_task)
  {
    var followers = get_task_followers(from_task.Id);
    var items = followers.Select(x => new TaskFollower
    {
      TaskId = to_task.Id,
      StaffId = x.Id
    }).ToList();
    db.TaskFollowers.AddRange(items);
  }

  public async void copy_task_assignees(Task from_task, Task to_task)
  {
    var assignees = get_task_assignees(from_task.Id);

    foreach (var assignee in assignees)
      db.TaskAssigneds.Add(new TaskAssigned
      {
        TaskId = to_task.Id,
        StaffId = assignee.Id,
        AssignedFrom = staff_user_id
      });
  }

  public void copy_task_checklist_items(Task from_task, Task to_task)
  {
    var checklists = get_checklist_items(from_task.Id);
    checklists = checklists.Select(x => new TaskChecklistItem
    {
      TaskId = to_task.Id,
      Finished = false,
      Description = x.Description,
      DateCreated = DateTime.Now,
      AddedFrom = x.AddedFrom,
      ListOrder = x.ListOrder,
      Assigned = x.Assigned
    }).ToList();
    db.TaskChecklistItems.AddRange(checklists);
    db.SaveChanges();
  }

  public void copy_task_custom_fields(Task from_task, Task to_task)
  {
    var custom_fields = db.get_custom_fields("tasks");
    foreach (var field in custom_fields)
    {
      var value = db.get_custom_field_value(from_task.Id, field.Id, "tasks", false);
      if (!string.IsNullOrEmpty(value))
        db.CustomFieldsValues.Add(new CustomFieldsValue
        {
          RelId = to_task.Id,
          FieldId = field.Id,
          FieldTo = "tasks",
          Value = value
        });
    }
  }

  public List<DataSet<Task>> get_billable_tasks(int? customer_id = null, int project_id = 0)
  {
    var has_permission_view = db.has_permission("tasks", "", "view");
    var noPermissionsQuery = self.helper.get_tasks_where_string(false);


    var query = db.Tasks
      .Include(x => x.Invoice)
      .Include(x => x.TaskAssigneds)
      .Include(x => x.TaskComments)
      .Include(x => x.TaskFollowers)
      .Include(x => x.TasksTimers)
      .Include(x => x.TaskChecklistItems)
      .Where(x =>
        x.Billable == 1 &&
        x.Billed == 0
      )
      .AsQueryable();

    if (project_id == 0)
      query = query.Where(x => x.RelType != "project");
    else
      query = query.Where(x =>
        x.RelType == "project" &&
        x.RelId == project_id
      );

    if (customer_id != 0 && project_id == 0)
      query = query.Where(x =>
        (db.Invoices.Where(y => y.ClientId == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "invoice") ||
        (db.Estimates.Where(y => y.ClientId == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "estimate") ||
        (db.Contracts.Where(y => y.Client == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "contract") ||
        (db.Tickets.Where(y => y.UserId == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "contract") ||
        (db.Expenses.Where(y => y.ClientId == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "contract") ||
        (db.Proposals.Where(y => y.RelId == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "contract") ||
        (db.Clients.Where(y => y.Id == customer_id).Select(y => y.Id).ToList().Contains(x.Id) && x.RelType == "contract")
      );

    if (!has_permission_view) query = query.Where(noPermissionsQuery);
    var tasks = query.ToList();
    var i = 0;
    return tasks.Select(task =>
      {
        var item = new DataSet<Task>();
        item.Data = task;

        var task_rel_data = self.helper.get_relation_data(task.RelType, task.RelId.Value);
        var task_rel_value = self.navigation.get_relation_values(convert<RelationValues>(task_rel_data), task.RelType);
        item.Options.Add(new Option()
        {
          Name = task_rel_value.name
        });
        item.Options.Add(new Option()
        {
          Name = "RelValue",
          Value = task_rel_value.rel_value
        });
        item.Options.Add(new Option()
        {
          Name = "StartedTimers",
          Value = db.TasksTimers.Any(x => x.TaskId == task.Id && x.EndTime == null) ? "true" : "false"
        });
        return item;
      })
      .ToList();
  }

  public object get_billable_amount(int taskId)
  {
    var data = get_billable_task_data(taskId);
    // var total_hours = total_hours(data);
    var total_hours = 0;
    return self.helper.app_format_number(total_hours * data.HourlyRate);
  }

  public Task get_billable_task_data(int task_id)
  {
    // var data = db.Tasks.FirstOrDefault(x => x.Id == task_id);
    var dataset = new DataSet<Task>
    {
      Data = db.Tasks.FirstOrDefault(x => x.Id == task_id) ?? new Task()
    };

    if (dataset.Data.RelType == "project")
    {
      var project = db.Projects.FirstOrDefault(x => x.Id == dataset.Data.RelId);
      var billing_type = db.get_project_billing_type(dataset.Data.RelId.Value);
      if (project.BillingType == 2) dataset.Data.HourlyRate = project.ProjectRatePerHour;
      dataset.Data.Name = $"{project.Name} - {dataset.Data.Name}";
    }

    var total_seconds = self.helper.task_timer_round(calc_task_total_time(task_id, x => true));
    // dataset.total_hours = self.helper.sec2qty(total_seconds);
    dataset["total_seconds"] = total_seconds;
    return convert<Task>(dataset);
  }

  public List<Task> get_tasks_by_staff_id(Expression<Func<Task, bool>> condition)
  {
    // return this.db.get( 'tasks').result_array();
    var output = db.Tasks
      .Where(condition)
      .Where(x => db.TaskAssigneds.Any(y => y.StaffId == x.Id))
      .ToList();


    return output;
  }

  /**
   * Add new staff task
   * @param array data task _POST data
   * @return mixed
   */
  public int add(DataSet<Task> dataset, bool clientRequest = false)
  {
    var assignees = new List<int>();
    var followers = new List<int>();
    var fromTicketId = 0;
    // var fromTicketId = data.TicketToTask;
    // var fromTicketId = new List<int>();
    // if (data.TicketToTask) fromTicketId = data.TicketToTask;
    dataset.Data.DateCreated = DateTime.Now;
    dataset.Data.AddedFrom = clientRequest == false ? staff_user_id : db.get_contact_user_id();
    dataset.Data.IsAddedFromContact = clientRequest;

    var checklistItems = new List<TaskChecklistItem>();
    if (dataset.Data.TaskChecklistItems.Any())
      checklistItems = dataset.Data.TaskChecklistItems.ToList();

    if (clientRequest == false)
    {
      var defaultStatus = db.get_option("default_task_status");
      dataset.Data.Status = defaultStatus == "auto"
        ? Convert.ToInt32(Convert.ToString(DateTime.Now >= dataset.Data.StartDate ? 4 : 1))
        : Convert.ToInt32(defaultStatus);
    }
    else
    {
      // When client create task the default status is NOT STARTED
      // After staff will get the task will change the status
      dataset.Data.Status = 1;
    }

    var custom_fields = new List<CustomField>();
    if (dataset.custom_fields.Any())
      custom_fields = dataset.custom_fields;

    if (string.IsNullOrEmpty(dataset.Data.RepeatEvery))
    {
      dataset.Data.Recurring = 1;
      if (dataset.Data.RepeatEvery == "custom")
      {
        dataset.Data.RepeatEvery = (string)dataset["repeat_every_custom"];
        dataset.Data.RecurringType = (string)dataset["repeat_type_custom"];
        dataset.Data.CustomRecurring = 1;
      }
      else
      {
        var _temp = dataset.Data.RepeatEvery.Split("-").ToList();
        dataset.Data.RecurringType = _temp[1];
        dataset.Data.RepeatEvery = _temp[0];
        dataset.Data.CustomRecurring = 0;
      }
    }
    else
    {
      dataset.Data.Recurring = 0;
      dataset.Data.RepeatEvery = null;
    }

    // if ( data.repeat_type_custom &&  data.repeat_every_custom) {
    //      (data.repeat_type_custom);
    //      (data.repeat_every_custom);
    // }

    if (db.is_client_logged_in() || clientRequest)
      dataset.Data.VisibleToClient = true;


    if (!dataset.Data.Milestone.HasValue)
    {
      dataset.Data.Milestone = 0;
    }
    else
    {
      if (dataset.Data.RelType != "project")
        dataset.Data.Milestone = 0;
    }

    if (string.IsNullOrEmpty(dataset.Data.RelType))
    {
      dataset.Data.RelType = null;
      dataset.Data.RelId = 0;
    }
    else
    {
      if (dataset.Data.RelId.HasValue)
      {
        dataset.Data.RelType = null;
        dataset.Data.RelId = 0;
      }
    }

    var withDefaultAssignee = true;
    if (dataset.ContainesKey("withDefaultAssignee"))
      withDefaultAssignee = (bool)dataset["withDefaultAssignee"];

    dataset.Data = hooks.apply_filters("before_add_task", dataset.Data);

    var tags = new List<Taggable>();
    if (dataset.Tags.Any()) tags = dataset.Tags;

    if (dataset.Data.TaskAssigneds.Any()) dataset.Data.TaskAssigneds.Clear();
    if (dataset.Data.TaskFollowers.Any()) dataset.Data.TaskFollowers.Clear();

    var result = db.Tasks.Add(dataset.Data);
    var insert_id = result.Entity.Id;
    if (insert_id == 0) return 0;
    foreach (var item in checklistItems)
    {
      var key = item.ListOrder;
      var chkID = item.Id;
      if (chkID <= 0) continue;
      var itemTemplate = get_checklist_template(chkID);
      db.TaskChecklistItems.Add(new TaskChecklistItem
      {
        Description = itemTemplate.Description,
        TaskId = insert_id,
        DateCreated = DateTime.Now,
        AddedFrom = staff_user_id,
        ListOrder = key
      });
    }

    custom_fields.Clear();
    db.handle_tags_save(tags, insert_id, "task");
    if (custom_fields.Any())
      self.helper.handle_custom_fields_post(insert_id, custom_fields);

    if (!string.IsNullOrEmpty(dataset.Data.RelType) && dataset.Data.RelType == "lead")
      leads_model.log_lead_activity(dataset.Data.RelId.Value, "not_activity_new_task_created", false, JsonConvert.SerializeObject(new[]
      {
        $"<a href='{self.navigation.admin_url($"tasks/view/{insert_id}")}' onclick='init_task_modal({insert_id});return false;'>{dataset.Data.Name}</a>"
      }));

    if (clientRequest == false)
    {
      assignees.ForEach(staff_id =>
      {
        add_task_assignees(new TaskAssigned
        {
          TaskId = insert_id,
          AssignedFrom = staff_id
        });
      });

      // else {
      //     new_task_auto_assign_creator = (db.get_option("new_task_auto_assign_current_member") == '1' ? true : false);

      //     if (  (data.RelType)
      //         && data.RelType == "project"
      //         && !this.projects_model.is_member(data.RelId)
      //         || !withDefaultAssignee
      //         ) {
      //         new_task_auto_assign_creator = false;
      //     }
      //     if (new_task_auto_assign_creator == true) {
      //         this.db.insert( 'task_assigned', [
      //             'taskid'        => insert_id,
      //             'staffid'       => staff_user_id,
      //             'assigned_from' => staff_user_id,
      //         ]);
      //     }
      // }

      if (followers.Any())
        followers.ForEach(staff_id =>
        {
          add_task_followers(new TaskFollower
          {
            TaskId = insert_id,
            StaffId = staff_id
          });
        });
      //  else {
      //     if (db.get_option("new_task_auto_follower_current_member") == '1') {
      //         this.db.insert( 'task_followers', [
      //             'taskid'  => insert_id,
      //             'staffid' => staff_user_id,
      //         ]);
      //     }
      // }

      if (fromTicketId != null)
      {
        var ticket_attachments =
          db.TicketAttachments.Where(x => x.TicketId == fromTicketId ||
                                          (x.TicketId == fromTicketId && db.TicketReplies.Any(y => y.TicketId == fromTicketId))).ToList();

        if (ticket_attachments.Any())
        {
          var task_path = $"{get_upload_path_by_type("task")}{insert_id}/";
          self.helper.maybe_create_upload_path(task_path);

          foreach (var ticket_attachment in ticket_attachments)
          {
            var path = $"{get_upload_path_by_type("ticket")}{fromTicketId}/{ticket_attachment.FileName}";
            if (!file_exists(path)) continue;

            var filename = unique_filename(task_path, ticket_attachment.FileName);

            // var fpt = self.helper.fopen(task_path + filename, 'w');
            // if (file_exists(filename) && self.helper.file_put_contents(filename))
            // {
            // }
// self.helper.file_put_contents(path,self.helper.get_upload_path_by_type
//             if (self.helper.fir fwrite(fpt, self.helper.stream_get_contents(f)))
            db.Files.Add(new File
            {
              RelId = insert_id,
              RelType = "task",
              FileName = filename,
              FileType = ticket_attachment.FileType,
              StaffId = staff_user_id,
              DateCreated = DateTime.Now,
              AttachmentKey = self.helper.uuid()
            });
          }
        }
      }
    }

    log_activity($"New Task Added [ID : {insert_id}, Name : {dataset.Data.Name}]");
    hooks.do_action("after_add_task", insert_id);
    return insert_id;
  }

  /**
   * Update task data
   * @param  array data task data _POST
   * @param  mixed id   task id
   * @return boolean
   */
  public bool update(TaskDto data, int id, bool clientRequest = false)
  {
    var affectedRows = 0;


    var checklistItems = new List<TaskChecklistItem>();
    if (data.TaskChecklistItems.Any())
      checklistItems = data.TaskChecklistItems.ToList();

    if (clientRequest == false)
    {
      data.Cycles = data.Cycles == 1 ? 0 : data.Cycles;

      var original_task = get(x => x.Id == id);

      // Recurring task set to NO, Cancelled
      if (original_task.RepeatEvery != "" && data.RepeatEvery == "")
      {
        data.Cycles = 0;
        data.TotalCycles = 0;
        data.LastRecurringDate = null;
      }

      if (data.RepeatEvery != "")
      {
        data.Recurring = 1;
        if (data.RepeatEvery == "custom")
        {
          data.RepeatEvery = data.repeat_every_custom;
          data.RecurringType = data.repeat_type_custom;
          data.CustomRecurring = 1;
        }
        else
        {
          var _temp = data.RepeatEvery.Split("-").ToList();
          data.RecurringType = _temp[1];
          data.RepeatEvery = _temp[0];
          data.CustomRecurring = 0;
        }
      }
      else
      {
        data.Recurring = 0;
      }
    }

    if (data.Milestone is null)
    {
      data.Milestone = 0;
    }
    else
    {
      if (data.RelType != "project") data.Milestone = 0;
    }


    if (string.IsNullOrEmpty(data.RelType))
    {
      data.RelId = null;
      data.RelType = null;
    }
    else
    {
      if (data.RelId.HasValue)
      {
        data.RelId = null;
        data.RelType = null;
      }
    }

    data = hooks.apply_filters("before_update_task", data, id);
    var custom_fields = new List<CustomField>();
    if (data.custom_fields.Any())
    {
      custom_fields = data.custom_fields;
      if (self.helper.handle_custom_fields_post(id, custom_fields))
        affectedRows++;
    }

    if (data.Tags.Any())
      if (db.handle_tags_save(data.Tags, id, "task"))
        affectedRows++;


    checklistItems.Select(x => x.Id).ToList()
      .ForEach(checklistItem =>
      {
        var itemTemplate = get_checklist_template(checklistItem);
        db.TaskChecklistItems.Add(new TaskChecklistItem
        {
          Description = itemTemplate.Description,
          TaskId = id,
          DateCreated = DateTime.Now,
          AddedFrom = staff_user_id,
          ListOrder = $"{checklistItem}/key"
        });
        affectedRows++;
      });

    var result = db.Tasks.Where(x => x.Id == id).Update(x => data);
    if (result > 0) affectedRows++;

    if (affectedRows <= 0) return false;
    hooks.do_action("after_update_task", id);
    log_activity($"Task Updated [ID:{id}, Name: {data.Name}]");
    return true;
  }

  public TaskChecklistItem? get_checklist_item(int id)
  {
    return db.TaskChecklistItems.Where(x => x.Id == id).FirstOrDefault();
  }

  public List<TaskChecklistItem> get_checklist_items(int taskid)
  {
    var rows = db.TaskChecklistItems
      .Where(x => x.TaskId == taskid)
      .OrderBy(x => x.ListOrder)
      .ToList();
    return rows;
  }


  public int add_checklist_template(TasksChecklistTemplate description)
  {
    var result = db.TasksChecklistTemplates
      .Add(description);

    return result.Entity.Id;
  }


  public bool remove_checklist_item_template(int id)
  {
    var result = db.TasksChecklistTemplates
      .Remove(new TasksChecklistTemplate { Id = id });
    return result.IsDeleted();
  }

  public List<TasksChecklistTemplate> get_checklist_templates()
  {
    var rows = db.TasksChecklistTemplates.OrderBy(x => x.Description).ToList();
    return rows;
  }

  public TasksChecklistTemplate get_checklist_template(int id)
  {
    var row = db.TasksChecklistTemplates.FirstOrDefault(x => x.Id == id);

    return row;
  }

  /**
   * Add task new blank check list item
   * @param mixed data _POST data with taxid
   */
  public bool add_checklist_item(TaskChecklistItem data)
  {
    data.DateCreated = DateTime.Now;
    data.AddedFrom = staff_user_id;
    var result = db.TaskChecklistItems.Add(data);


    if (result.State != EntityState.Added) return false;
    hooks.do_action("task_checklist_item_created", new { task_id = data.TaskId, checklist_id = data.Id });
    return true;
  }

  public bool delete_checklist_item(int id)
  {
    var result = db.TaskChecklistItems.Where(x => x.Id == id).Delete();
    return result > 0;
  }

  public void update_checklist_order(params TaskChecklistItem[] data)
  {
    data
      .ToList()
      .ForEach(x =>
      {
        db.TaskChecklistItems
          .Where(x => x.Id == x.Id)
          .Update(x => new TaskChecklistItem
          {
            ListOrder = x.ListOrder
          });
      });
  }

  /**
   * Update checklist item
   * @param  mixed id          check list id
   * @param  mixed description checklist description
   * @return void
   */
  public void update_checklist_item(int id, string description)
  {
    // description = self.helper.strip_tags(description, "<br>,<br/>");
    description = self.helper.strip_tags(description);
    if (string.IsNullOrEmpty(description))
      db.TaskChecklistItems
        .Where(x => x.Id == id)
        .DeleteAsync();
    else
      db.TaskChecklistItems
        .Where(x => x.Id == id)
        .Update(x => new TaskChecklistItem
        {
          Description = description.nl2br()
        });
  }

  /**
   * Make task public
   * @param  mixed task_id task id
   * @return boolean
   */
  public async Task<bool> make_public(int task_id)
  {
    var result = await db.Tasks
      .Where(x => x.Id == task_id)
      .UpdateAsync(x => new Task
      {
        IsPublic = true
      });
    await db.SaveChangesAsync();
    // return result.State == EntityState.Modified;
    return result > 0;
  }


  /**
   * Get task creator id
   * @param  mixed taskid task id
   * @return mixed
   */
  public int get_task_creator_id(int taskid)
  {
    var row = db.Tasks.FirstOrDefault(x => x.Id == taskid);
    return row.AddedFrom;
  }

  /**
   * Add new task comment
   * @param array data comment _POST data
   * @return boolean
   */
  public int add_task_comment(TaskComment data)
  {
    if (db.is_client_logged_in())
    {
      data.StaffId = 0;
      data.ContactId = db.get_contact_user_id();
    }
    else
    {
      data.StaffId = staff_user_id;
      data.ContactId = 0;
    }

    var result = db.TaskComments.Add(new TaskComment
    {
      TaskId = data.Id,
      //'content'    =>db.is_client_logged_in() ? _strip_tags(data.Content) : data.Content,
      Content = data.Content,
      StaffId = data.StaffId,
      ContactId = data.ContactId,
      DateCreated = DateTime.Now
    });
    var insert_id = result.Entity.Id;

    if (result.State != EntityState.Added) return 0;
    var task = db.Tasks.FirstOrDefault(x => x.Id == data.Id);

    var description = "not_task_new_comment";
    var additional_data = JsonConvert.SerializeObject(new { task.Name });

    if (task.RelType == "project") projects_model.log(task.RelId ?? 0, "project_activity_new_task_comment", task.Name, task.VisibleToClient);

    var pattern = @"data-mention-id='(\d+)'";

    // Display matches
    foreach (var match in preg_match_all(pattern, data.Content))
    {
      var temps = match.Select(int.Parse).ToList();
      Console.WriteLine(string.Join(", ", match));
      if (preg_match_all(pattern, data.Content).Any())
      {
        _send_task_mentioned_users_notification(
          description,
          data.Id,
          temps,
          "task_new_comment_to_staff",
          additional_data,
          insert_id
        );
      }
      else
      {
        _send_task_responsible_users_notification(
          description,
          data.Id,
          null,
          "task_new_comment_to_staff",
          additional_data,
          insert_id
        );


        var project_settings = db.ProjectSettings.FirstOrDefault(x =>
          x.ProjectId == task.RelId &&
          x.Name == "view_task_comments"
        );

        if (project_settings != null && Convert.ToInt32(project_settings.Value) == 1)
          _send_customer_contacts_notification(data.TaskId, "task_new_comment_to_customer");
      }
    }

    hooks.do_action("task_comment_added", new { task_id = data.TaskId, comment_id = insert_id });
    return insert_id;
  }

  /**
   * Add task followers
   * @param array data followers _POST data
   * @return boolean
   */
  public async Task<bool> add_task_followers(TaskFollower data)
  {
    data.TaskId = data.TaskId;
    data.StaffId = data.StaffId;
    var result = db.TaskFollowers.Add(data);

    if (result.State != EntityState.Added) return false;
    var taskName = self.helper.get_task_subject_by_id(data.TaskId);

    if (staff_user_id != data.Id)
    {
      var notified = db.add_notification(new Notification
      {
        Description = "not_task_added_you_as_follower",
        ToUserId = data.StaffId,
        Link = $"#taskid={data.TaskId}",
        AdditionalData = JsonConvert.SerializeObject(new List<string> { taskName })
      });

      if (notified)
        db.pusher_trigger_notification(data.Task.TaskFollowers.Select(x => x.Id).ToList());

      var member = staff_model.get(x => x.TaskFollowers == data.Task.TaskFollowers).First();

      self.helper.send_mail_template(
        "task_added_as_follower_to_staff",
        member.Email,
        data.Task.TaskFollowers,
        data.TaskId
      );
    }

    var description = "not_task_added_someone_as_follower";

    var additional_notification_data = JsonConvert.SerializeObject(new[]
    {
      db.get_staff_full_name(data.Task.TaskFollowers.First().Id),
      taskName
    });

    if (data.Task.TaskFollowers.First().StaffId == staff_user_id)
    {
      additional_notification_data = JsonConvert.SerializeObject(new[] { taskName });
      description = "not_task_added_himself_as_follower";
    }

    _send_task_responsible_users_notification(
      description,
      data.TaskId,
      data.Task.TaskFollowers.First().Id,
      "",
      additional_notification_data
    );

    hooks.do_action("task_follower_added", new TaskFollower
    {
      TaskId = data.TaskId,
      StaffId = data.StaffId
    });

    return true;
  }

  /**
   * Assign task to staff
   * @param array data task assignee _POST data
   * @return boolean
   */
  public void add_task_assignees(TaskAssigned data, bool cronOrIntegration = false, bool clientRequest = false)
  {
    var assignData = new TaskAssigned
    {
      TaskId = data.TaskId,
      StaffId = data.AssignedFrom
    };
    if (cronOrIntegration)
    {
      assignData.AssignedFrom = data.AssignedFrom;
    }
    else if (clientRequest)
    {
      assignData.IsAssignedFromContact = 1;
      assignData.AssignedFrom = db.get_contact_user_id();
    }
    else
    {
      assignData.AssignedFrom = staff_user_id;
    }

    var result = db.TaskAssigneds.Add(new TaskAssigned
    {
      TaskId = data.TaskId,
      StaffId = data.AssignedFrom,
      AssignedFrom = assignData.AssignedFrom
    });

    if (result.State != EntityState.Added) return;
    var task = db.Tasks.FirstOrDefault(x => x.Id == data.TaskId);

    if (staff_user_id != data.AssignedFrom || clientRequest)
    {
      var notification_data = new Notification
      {
        Description = cronOrIntegration == false ? "not_task_assigned_to_you" : "new_task_assigned_non_user",
        ToUserId = data.AssignedFrom,
        Link = $"#taskid={data.TaskId}",
        AdditionalData = JsonConvert.SerializeObject(new List<string> { task.Name })
      };

      if (cronOrIntegration) notification_data.FromCompany = true;

      if (clientRequest) notification_data.FromClientId = db.get_contact_user_id();

      if (db.add_notification(notification_data)) db.pusher_trigger_notification(new List<int> { data.AssignedFrom });

      var member = staff_model.get(x => x.Id == data.AssignedFrom).FirstOrDefault();

      self.helper.send_mail_template("task_assigned_to_staff", member.Email, data.AssignedFrom, data.TaskId);
    }

    var description = "not_task_assigned_someone";
    var additional_notification_data = JsonConvert.SerializeObject(new[]
    {
      db.get_staff_full_name(data.AssignedFrom),
      task.Name
    });
    if (data.AssignedFrom == staff_user_id)
    {
      description = "not_task_will_do_user";
      additional_notification_data = JsonConvert.SerializeObject(new[]
      {
        task.Name
      });
    }

    if (task.RelType == "project")
      projects_model.log(task.RelId ?? 0, "project_activity_new_task_assignee", $"{task.Name} - {db.get_staff_full_name(data.StaffId)}", task.VisibleToClient);

    _send_task_responsible_users_notification(
      description,
      data.TaskId,
      data.AssignedFrom,
      "",
      additional_notification_data
    );

    hooks.do_action("task_assignee_added", new TaskAssigned
    {
      TaskId = data.TaskId,
      StaffId = data.AssignedFrom,
      AssignedFrom = assignData.AssignedFrom
    });
    // return assigneeId;
  }

  /**
   * Get all task attachments
   * @param  mixed taskid taskid
   * @return array
   */
  public List<File> get_task_attachments(Expression<Func<File, bool>> where)
  {
    var rows =
      db.Files.Where(where)
        .Include(x => x.TaskCommentId)
        // Include(x=>x.Task)
        .Where(where)
        .Where(x => x.RelType == "task")
        .OrderByDescending(x => x.DateCreated)
        .ToList();
    return rows;
  }

  /**
   * Remove task attachment from server and database
   * @param  mixed id attachmentid
   * @return boolean
   */
  public void remove_task_attachment(int id)
  {
    var comment_removed = 0;
    var deleted = false;
    // Get the attachment
    var attachment = db.Files.FirstOrDefault(x => x.Id == id);

    if (attachment != null)
    {
      if (string.IsNullOrEmpty(attachment.External))
      {
        var relPath = $"{get_upload_path_by_type("task")}{attachment.RelId}/";
        var fullPath = relPath + attachment.FileName;
        unlink(fullPath);
        var fname = file_name(fullPath);
        var fext = self.helper.file_extension(fullPath);
        var thumbPath = $"{relPath}{fname}_thumb.{fext}";
        if (file_exists(thumbPath))
          unlink(thumbPath);
      }

      var affected_rows = db.Files.Where(x => x.Id == id).Delete();
      if (affected_rows > 0)
      {
        deleted = true;
        log_activity($"Task Attachment Deleted [TaskID: {attachment.RelId}]");
      }

      if (is_dir(get_upload_path_by_type("task") + attachment.RelId))
      {
        // Check if no attachments left, so we can delete the folder also
        var other_attachments = list_files(get_upload_path_by_type("task") + attachment.RelId);
        if (!other_attachments.Any())
          // okey only index.html so we can delete the folder also
          delete_dir(get_upload_path_by_type("task") + attachment.RelId);
      }
    }

    if (!deleted) return;
    if (attachment.TaskCommentId == 0) return;
    var total_comment_files = db.Files.Any(x => x.TaskCommentId == attachment.TaskCommentId);
    if (!total_comment_files)
    {
      var comment = db.TaskComments.FirstOrDefault(x => x.Id == attachment.TaskCommentId);

      if (comment != null)
      {
        // Comment is empty and uploaded only with attachments
        // Now all attachments are deleted, we need to delete the comment too
        if (string.IsNullOrEmpty(comment.Content) || comment.Content == "[task_attachment]")
        {
          db.TaskComments.Where(x => x.Id == attachment.TaskCommentId).Delete();
          comment_removed = comment.Id;
        }
        else
        {
          db.TaskComments
            .Where(x => x.Id == attachment.TaskCommentId)
            .Update(x => new TaskComment
            {
              Content = x.Content.Replace("[task_attachment]", "")
            });
        }
      }
    }


    var comment_attachment = db.TaskComments.FirstOrDefault(x => x.FileId == id);

    if (comment_attachment != null) remove_comment(comment_attachment.Id);


    // return ['success' => deleted, 'comment_removed' => comment_removed];
  }

  /**
   * Add uploaded attachments to database
   * @since  Version 1.0.1
   * @param mixed taskid     task id
   * @param array attachment attachment data
   */
  public bool add_attachment_to_database(int rel_id, File attachment, string? external = null, bool notification = true)
  {
    var file_id = misc_model.add_attachment_to_database(rel_id, "task", attachment, external);
    if (file_id == 0) return false;

    var task = db.Tasks.Where(x => x.Id == rel_id).FirstOrDefault();

    if (task.RelType == "project") projects_model.log(task.RelId ?? 0, "project_activity_new_task_attachment", task.Name, task.VisibleToClient);

    if (notification)
    {
      var description = "not_task_new_attachment";
      _send_task_responsible_users_notification(description, rel_id, null, "task_new_attachment_to_staff");
      _send_customer_contacts_notification(rel_id, "task_new_attachment_to_customer");
    }

    var task_attachment_as_comment = hooks.apply_filters("add_task_attachment_as_comment", true);

    if (task_attachment_as_comment != true) return true;
    var file = misc_model.get_file(file_id);


    var result = db.TaskComments
      .Add(new TaskComment
      {
        Content = "[task_attachment]",
        TaskId = rel_id,
        StaffId = file.StaffId.Value,
        ContactId = file.ContactId,
        FileId = file_id,
        DateCreated = DateTime.Now
      });

    return result.IsAdded();
  }

  /**
   * Get all task followers
   * @param  mixed id task id
   * @return array
   */
  public List<TaskFollower> get_task_followers(int id)
  {
    var rows = db.TaskFollowers
      .Include(x => x.Staff)
      .Where(x => x.TaskId == id)
      .ToList();


    return rows;
  }

  /**
   * Get all task assigneed
   * @param  mixed id task id
   * @return array
   */
  public List<TaskAssigned> get_task_assignees(int id)
  {
    var rows = db.TaskAssigneds
      .Include(x => x.Staff)
      .Where(x => x.TaskId == id)
      .OrderBy(x => x.Staff.FirstName)
      .ToList();
    return rows;
  }

  /**
   * Get task comment
   * @param  mixed id task id
   * @return array
   */
  public List<TaskComment> get_task_comments(int id)
  {
    var task_comments_order = hooks.apply_filters("task_comments_order", "DESC");

    var comments = db.TaskComments
      .Include(x => x.Staff)
      .Include(taskComment => taskComment.Contact)
      .Where(x => x.TaskId == id)
      .OrderByDescending(x => x.DateCreated).ToList();

    var comments_dto = comments
      .Select(x => new TaskComment
      {
        Id = x.Id,
        TaskId = x.TaskId,
        StaffId = x.StaffId,
        ContactId = x.ContactId,
        Content = x.Content,
        DateCreated = x.DateCreated,
        Staff = x.Staff,
        Contact = x.Contact
      }).ToList();
    var ids = comments.Select(x => x.Id).ToList();
    // comments.ForEach(comment => { comment.attachments = new List<object>(); });
    if (!ids.Any()) return comments;
    var allAttachments = get_task_attachments(x => x.Id == id && ids.Contains(x.TaskCommentId!.Value));
    allAttachments.ForEach(comment =>
    {
      allAttachments.ForEach(attachment =>
      {
        //
        var comment = comments.FirstOrDefault(x => x.Id == attachment.TaskCommentId);
        // comment.Attachments.Add(attachment);
        //
      }); //
    });
    return comments;
  }

  public async Task<bool> edit_comment(TaskComment data)
  {
    // Check if user really creator

    var comment = db.TaskComments.FirstOrDefault(x => x.Id == data.Id);

    var edit_tasks = db.has_permission("tasks", "", "edit");
    if (comment.StaffId != staff_user_id && !edit_tasks && comment.ContactId != db.get_contact_user_id())
      return false;

    var comment_added = comment.DateCreated;
    var minus_1_hour = DateTime.Now.AddHours(-1);
    if (!db.get_option_compare("client_staff_add_edit_delete_task_comments_first_hour", 0) && (!db.get_option_compare("client_staff_add_edit_delete_task_comments_first_hour", 1) || comment_added < minus_1_hour) && !db.is_admin()) return false;

    if (db.Files.Any(x => x.TaskCommentId == comment.Id)) data.Content += "[task_attachment]";
    var affected_rows = await db.TaskComments
      .Where(x => x.Id == data.Id)
      .UpdateAsync(x => new TaskComment { Content = data.Content });
    if (affected_rows <= 0) return false;
    hooks.do_action("task_comment_updated", new TaskComment
    {
      Id = comment.Id,
      TaskId = comment.TaskId
    });

    return true;
  }

  /**
   * Remove task comment from database
   * @param  mixed id task id
   * @return boolean
   */
  public bool remove_comment(int id, bool force = false)
  {
    // Check if user really creator

    var comment = db.TaskComments.FirstOrDefault(x => x.Id == id);

    if (comment == null) return true;

    var task_delete = db.has_permission("tasks", "", "delete");
    var contact_user_id = db.get_contact_user_id();
    if (comment.StaffId != staff_user_id && !task_delete && comment.ContactId != contact_user_id && force != true) return false;
    {
      var comment_added = comment.DateCreated;
      var minus_1_hour = DateTime.Now.AddHours(-1);
      if (!db.get_option_compare("client_staff_add_edit_delete_task_comments_first_hour", 0) &&
          (!db.get_option_compare("client_staff_add_edit_delete_task_comments_first_hour", 1) || comment_added < minus_1_hour)
          && !db.is_admin() && !force) return false;
      var result = db.TaskComments.Where(x => x.Id == id).Delete();
      if (result == 0) return false;
      if (comment.FileId != 0) remove_task_attachment(comment.FileId ?? 0);
      var commentAttachments = get_task_attachments(x => x.Id == comment.TaskId && x.TaskCommentId == id);
      commentAttachments.ForEach(attachment => remove_task_attachment(attachment.Id));
      hooks.do_action("task_comment_deleted", new { task_id = comment.TaskId, comment_id = id });
      return true;
    }
  }

  /**
   * Remove task assignee from database
   * @param  mixed id     assignee id
   * @param  mixed taskid task id
   * @return boolean
   */
  public bool remove_assignee(int id, int taskid)
  {
    var task = db.Tasks.FirstOrDefault(x => x.Id == id);
    var assignee_data = db.TaskAssigneds.FirstOrDefault(x => x.Id == id);

    // Delete timers
    //   this.db.where('task_id', taskid);
    ////   this.db.where('staff_id', assignee_data.staffid);
    ///   this.db.delete('taskstimers');

    // Stop all timers for this task and assignee
    var result = db.TasksTimers
      .Where(x =>
        x.TaskId == taskid &&
        x.StaffId == assignee_data.StaffId &&
        x.EndTime == null
      )
      .Update(x => new TasksTimer
      {
        EndTime = DateTime.Now
      });

    result = db.TaskAssigneds.Where(x => x.Id == id).Delete();
    if (result == 0) return false;
    if (task.RelType == "project")
      projects_model.log(task.RelId ?? 0, "project_activity_task_assignee_removed", $"{task.Name} - {db.get_staff_full_name(assignee_data.StaffId)}", task.VisibleToClient);

    return true;
  }

  /**
   * Remove task follower from database
   * @param  mixed id     followerid
   * @param  mixed taskid task id
   * @return boolean
   */
  public bool remove_follower(int id, int taskid)
  {
    var result = db.TaskFollowers.Where(x => x.Id == id).Delete();
    return result > 0;
  }

  /**
   * Change task status
   * @param  mixed status  task status
   * @param  mixed task_id task id
   * @return boolean
   */
  public bool mark_as(int status, int task_id)
  {
    var sender = new DataSet<Task>();
    sender.Data = db.Tasks.FirstOrDefault(x => x.Id == task_id);
    sender.Data.Status = status;
    return mark_as(sender, task_id);
  }

  public bool mark_as(DataSet<Task> status, int task_id)
  {
    var task = db.Tasks.FirstOrDefault(x => x.Id == task_id);
    if (task.Status == STATUS_COMPLETE) return unmark_complete(task_id, status.Data.Status);

    var _update = new Task { Status = status.Data.Status };

    if (status.Data.Status == STATUS_COMPLETE) _update.DateFinished = DateTime.Now;
    var result = db.Tasks
      .Where(x => x.Id == task_id)
      .Update(x => new Task
      {
        Status = status.Data.Status,
        DateFinished = DateTime.Now
      });
    if (result == 0) return false;
    var description = "not_task_status_changed";
    var not_data = new[]
    {
      task.Name,
      self.helper.format_task_status(status, false, true)
    };

    if (status.Data.Status == STATUS_COMPLETE)
    {
      description = "not_task_marked_as_complete";
      //  (not_data[1]);
      db.TasksTimers
        .Where(x =>
          x.TaskId == task_id &&
          x.EndTime == null
        )
        .Update(x => new TasksTimer
        {
          EndTime = DateTime.Now
        });
    }

    if (task.RelType == "project")
    {
      var project_activity_log = status.Data.Status == STATUS_COMPLETE ? "project_activity_task_marked_complete" : "not_project_activity_task_status_changed";
      var project_activity_desc = task.Name;

      if (status.Data.Status != STATUS_COMPLETE) project_activity_desc += $" - {self.helper.format_task_status(status)}";
      projects_model.log(task.RelId ?? 0, project_activity_log, project_activity_desc, task.VisibleToClient);
    }

    _send_task_responsible_users_notification(description, task_id, null, "task_status_changed_to_staff", JsonConvert.SerializeObject(not_data));

    _send_customer_contacts_notification(task_id, "task_status_changed_to_customer");
    hooks.do_action("task_status_changed", new Task
    {
      Id = task_id,
      Status = status.Data.Status
    });

    return true;
  }

  /**
   * Unmark task as complete
   * @param  mixed id task id
   * @return boolean
   */
  public bool unmark_complete(int id, int? force_to_status = null)
  {
    var status = 0;
    if (force_to_status.HasValue)
    {
      status = force_to_status.Value;
    }
    else
    {
      status = 1;
      var _task = db.Tasks.Where(x => x.Id == id).FirstOrDefault();
      if (_task.StartDate < DateTime.Now)
        status = 4;
    }

    var affected_rows = db.Tasks
      .Where(x => x.Id == id)
      .Update(x => new Task
      {
        DateFinished = null,
        Status = status
      });

    if (affected_rows <= 0) return false;


    var task = db.Tasks.FirstOrDefault(x => x.Id == id);

    if (task.RelType == "project")
      projects_model.log(task.RelId ?? 0, "project_activity_task_unmarked_complete", task.Name, task.VisibleToClient);

    var description = "not_task_unmarked_as_complete";

    _send_task_responsible_users_notification("not_task_unmarked_as_complete", id, null, "task_status_changed_to_staff", JsonConvert.SerializeObject(new
    {
      task.Name
    }));

    hooks.do_action("task_status_changed", new { status, task_id = id });

    return true;
  }

  /**
   * Delete task and all connections
   * @param  mixed id taskid
   * @return boolean
   */
  public bool delete_task(int id, bool log_activity = true)
  {
    var task = db.Tasks.FirstOrDefault(x => x.Id == id);
    var affected_rows = db.Tasks.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;

    // Log activity only if task is deleted indivudual not when deleting all projects
    if (task.RelType == "project" && log_activity) projects_model.log(task.RelId ?? 0, "project_activity_task_deleted", task.Name, task.VisibleToClient);
    db.TaskFollowers.Where(x => x.TaskId == id).Delete();
    db.TaskAssigneds.Where(x => x.TaskId == id).Delete();
    db.TaskComments.Where(x => x.TaskId == id).ToList().ForEach(x => { remove_comment(x.Id, true); });

    db.TaskChecklistItems.Where(x => x.TaskId == id).Delete();

    // Delete the custom field values
    db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "tasks").Delete();


    db.TasksTimers.Where(x => x.TaskId == id).Delete();
    db.Taggables.Where(x => x.RelId == id && x.RelType == "task").Delete();
    db.Reminders.Where(x => x.RelId == id && x.RelType == "task").Delete();


    db.Files.Where(x => x.RelId == id && x.RelType == "task").ToList().ForEach(x => { remove_task_attachment(x.Id); });

    db.RelatedItems.Where(x => x.RelId == id && x.RelType == "task").Delete();

    if (is_dir(get_upload_path_by_type("task") + id))
      delete_dir(get_upload_path_by_type("task") + id);


    db.UserMeta.Where(x => x.MetaKey == $"task-hide-completed-items-{id}").Delete();

    hooks.do_action("task_deleted", id);

    return true;
  }

  /**
   * Send notification on task activity to creator,follower/s,assignee/s
   * @param  string  description notification description
   * @param  mixed  taskid      task id
   * @param  boolean excludeid   excluded staff id to not send the notifications
   * @return boolean
   */
  private void _send_task_responsible_users_notification(string description, int taskid, int? excludeid = null, string email_template = "", string additional_notification_data = "", int? comment_id = null)
  {
    var notifiedUsers = new List<int>();
    staff_model.get(x => x.Active.Value)
      .ToList()
      .ForEach(x =>
      {
        if (excludeid.HasValue)
          if (excludeid == x.Id)
            return;
        if (!db.is_client_logged_in())
          if (x.Id == staff_user_id)
            return;

        if (!should_staff_receive_notification(x.Id, taskid)) return;
        var link = $"#taskid={taskid}";
        if (comment_id != 0) link += $"#comment_{comment_id}";

        var notified = db.add_notification(new Notification
        {
          Description = description,
          ToUserId = x.Id,
          Link = link,
          AdditionalData = additional_notification_data
        });

        if (notified != null) notifiedUsers.Add(x.Id);

        if (email_template != "") self.helper.send_mail_template(email_template, x.Email, x.Id, taskid);
      });

    db.pusher_trigger_notification(notifiedUsers);
  }

  public void _send_customer_contacts_notification(int taskid, string template_name)
  {
    var task = db.Tasks.FirstOrDefault(x => x.Id == taskid);

    if (task.RelType != "project") return;

    var project_settings = db.ProjectSettings.FirstOrDefault(x => x.ProjectId == task.RelId && x.Name == "view_tasks");
    if (project_settings == null) return;

    if (!string.IsNullOrEmpty(project_settings.Value) && task.VisibleToClient)
      clients_model.get_contacts_for_project_notifications(project_settings.ProjectId, "task_emails")
        .ToList()
        .ForEach(x =>
        {
          if (db.is_client_logged_in() && db.get_contact_user_id() == x.Id) return;
          self.helper.send_mail_template(template_name, x.Email, 0, taskid);
        });
  }

  /**
   * Check if user has commented on task
   * @param  mixed userid staff id to check
   * @param  mixed taskid task id
   * @return boolean
   */
  public bool staff_has_commented_on_task(int userid, int taskid)
  {
    return db.TaskComments
      .Any(x => x.StaffId == userid && x.TaskId == taskid);
  }

  /**
   * Check is user is task follower
   * @param  mixed  userid staff id
   * @param  mixed  taskid taskid
   * @return boolean
   */
  public bool is_task_follower(int userid, int taskid)
  {
    return db.TaskFollowers
      .Any(x => x.StaffId == userid && x.TaskId == taskid);
  }

  /**
   * Check is user is task assignee
   * @param  mixed  userid staff id
   * @param  mixed  taskid taskid
   * @return boolean
   */
  public bool is_task_assignee(int userid, int taskid)
  {
    var output = db.TaskAssigneds
      .Any(x => x.StaffId == userid && x.TaskId == taskid);
    return output;
  }

  /**
   * Check is user is task creator
   * @param  mixed  userid staff id
   * @param  mixed  taskid taskid
   * @return boolean
   */
  public bool is_task_creator(int userid, int taskid)
  {
    return db.Tasks
      .Any(x => x.AddedFrom == userid && x.Id == taskid &&
                x.IsAddedFromContact == false);
  }

  /**
   * Timer action, START/STOP Timer
   * @param  mixed  task_id   task id
   * @param  mixed  timer_id  timer_id to stop the timer
   * @param  string  note      note for timer
   * @param  boolean adminStop is admin want to stop timer from another staff member
   * @return boolean
   */
  public bool timer_tracking(int task_id = 0, int? timer_id = null, string? note = null, bool adminStop = false)
  {
    if (task_id == 0 && !timer_id.HasValue) return false;

    if (task_id != 0 && adminStop == false)
    {
      if (!is_task_assignee(staff_user_id, task_id))
        return false;
      if (is_task_billed(task_id)) return false;
    }

    var timer = get_task_timer(x => x.Id == timer_id);
    var newTimer = false || timer == null;

    if (newTimer)
    {
      var row = db.Staff.FirstOrDefault(x => x.Id == staff_user_id);
      var hourly_rate = row.HourlyRate;
      var result = db.TasksTimers.Add(new TasksTimer
      {
        StartTime = DateTime.Now,
        StaffId = staff_user_id,
        TaskId = task_id,
        HourlyRate = hourly_rate,
        Note = note
      });


      var _new_timer_id = result.Entity.Id;

      if (db.get_option_compare("auto_stop_tasks_timers_on_new_timer", 1))
        db.TasksTimers
          .Where(x =>
            x.Id != _new_timer_id &&
            x.EndTime == null &&
            x.TaskId != 0 &&
            x.StaffId == staff_user_id)
          .Update(x => new TasksTimer
          {
            EndTime = DateTime.Now,
            Note = note != "" ? note : null
          });

      if (
        task_id != '0'
        && db.get_option_compare("timer_started_change_status_in_progress", 1)
        && db.Tasks.Any(x => x.Id == task_id && x.Status == 1)
      )
        mark_as(STATUS_IN_PROGRESS, task_id);

      hooks.do_action("task_timer_started", new { id = task_id, timer_id = _new_timer_id });

      return true;
    }

    if (timer == null) return true;

    // time already ended
    if (timer.EndTime != null) return false;

    // var end_time = hooks.apply_filters<DateTime>("before_task_timer_stopped", today(), new { timer, task_id, note });
    var end_time = DateTime.Now;

    db.TasksTimers
      .Where(x => x.Id == timer_id)
      .Update(x => new TasksTimer
      {
        EndTime = end_time,
        TaskId = task_id,
        Note = note != "" ? note : null
      });


    return true;
  }

  public bool timesheet(TasksTimerDto data, params int[] duration_array)
  {
    var start_time = data.StartTime;
    var end_time = data.EndTime;
    if (duration_array.ToList().Any())
    {
      var hours = duration_array[0];
      var minutes = duration_array[1];
      end_time = DateTime.Now;
      start_time = DateTime.Now.AddHours(-hours).AddMinutes(-minutes);
    }

    if (end_time < start_time) return true;
    var timesheet_staff_id = staff_user_id;
    if (data.timesheet_staff_id > 0) timesheet_staff_id = data.timesheet_staff_id;

    if (data.Id > 0)
    {
      // Stop all other timesheets when adding new timesheet
      db.TasksTimers
        .Where(x =>
          // x.TaskId==data.timesheet_task_id &&
          x.StaffId == timesheet_staff_id &&
          x.EndTime == null)
        .Update(x =>
          new TasksTimer { EndTime = DateTime.Now }
        );


      var hourly_rate =
        db.Staff.FirstOrDefault(x => x.Id == timesheet_staff_id).HourlyRate;
      var insert_id = db.TasksTimers
        .Add(new TasksTimer
        {
          StartTime = start_time,
          EndTime = end_time,
          StaffId = timesheet_staff_id,
          TaskId = data.timesheet_task_id,
          HourlyRate = hourly_rate,
          Note = string.IsNullOrEmpty(data.Note) && data.Note != "" ? data.Note.nl2br() : null
        }).Entity.Id;
      var tags = new List<Taggable>();

      if (data.Tags.Any()) tags = data.Tags;

      db.handle_tags_save(tags, insert_id, "timesheet");

      if (insert_id == 0) return false;

      var task = db.Tasks.Where(x => x.Id == data.timesheet_task_id).FirstOrDefault();

      if (task.RelType != "project") return true;
      var total = end_time - start_time;
      var additional = $"<seconds>{total}</seconds>";
      additional += "<br />";
      additional += "<lang>project_activity_task_name</lang>{task.Name}";
      projects_model.log(task.RelId ?? 0, "project_activity_recorded_timesheet", additional, task.VisibleToClient);

      return true;
    }

    var affectedRows = 0;
    var result = db.TasksTimers
      .Where(x => x.Id == data.Id)
      .Update(x =>
        new TasksTimer
        {
          StartTime = start_time,
          EndTime = end_time,
          StaffId = timesheet_staff_id,
          TaskId = data.TaskId,
          Note = data.Note != "" ? data.Note.nl2br() : null
        });


    if (result > 0) affectedRows++;
    if (!data.Tags.Any()) return affectedRows > 0;
    if (db.handle_tags_save(data.Tags, data.Id, "timesheet")) affectedRows++;

    return affectedRows > 0;
  }

  public List<TasksTimer> get_timers(int task_id, Expression<Func<TasksTimer, bool>> where)
  {
    var rows = db.TasksTimers
      .Where(x => x.TaskId == task_id)
      .OrderByDescending(x => x.StartTime)
      .ToList();
    return rows;
  }

  public TasksTimer? get_task_timer(Expression<Func<TasksTimer, bool>> where)
  {
    var row = db.TasksTimers.Where(where).FirstOrDefault();
    return row;
  }

  public TasksTimer? is_timer_started(int task_id, int? staff_id = null)
  {
    staff_id ??= staff_user_id;

    var timer = get_last_timer(task_id, staff_id);

    if (timer != null || timer.EndTime != null) return null;

    return timer;
  }

  public bool is_timer_started_for_task(int id, Expression<Func<TasksTimer, bool>> condition)
  {
    var result =
      db.TasksTimers
        .Where(condition)
        .Where(x => x.EndTime == null && x.TaskId == id)
        .Any();

    return result;
  }

  public TasksTimer get_last_timer(int task_id, int? staff_id = null)
  {
    staff_id ??= staff_user_id;
    var timer = db.TasksTimers
      .Where(x => x.TaskId == task_id && x.StaffId == staff_id)
      .OrderByDescending(x => x.StartTime)
      .FirstOrDefault();


    return timer;
  }

  public Chart task_tracking_stats(int id)
  {
    var loggers = db.TasksTimers
      .GroupBy(t => t.Id) // Group by Id
      .Select(g => g.First()) // Select first tag in each group
      .ToList();
    var labels = loggers.Select(x => db.get_staff_full_name(x.StaffId)).ToList();
    var labels_ids = loggers.Select(x => x.StaffId).ToList();
    var chart = new Chart
    {
      Labels = labels,
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = self.helper.label("task_stats_logged_hours"),
          Data = new List<int>()
        }
      }
    };

    // foreach (var staffid in labels_ids ) {
    //     chart.Datasets[0].Data[i] = sec2qty(calc_task_total_time(id, ' AND staff_id='+staffid));
    //     i++;
    // }

    return chart;
  }

  public List<TasksTimer> get_timesheets(int task_id)
  {
    var rows = db.TasksTimers
      .Include(x => x.Staff)
      .Where(x => x.TaskId == task_id)
      .OrderByDescending(x => x.StartTime)
      .ToList();
    return rows;
  }

  public decimal get_time_spent(decimal seconds)
  {
    var minutes = seconds / 60;
    var hours = minutes / 60;
    if (minutes >= 60)
      return Math.Round(hours, 2);
    return seconds > 60
      ? Math.Round(minutes, 2)
      : seconds;
  }

  public int calc_task_total_time(int task_id, Expression<Func<Task, bool>> where)
  {
    // var sql = get_sql_calc_task_logged_time(task_id) + where;
    // var result = db.query(sql).row();
    // if (result) return result.total_logged_time;

    return 0;
  }

  public List<int> get_unique_member_logged_task_ids(int staff_id, Expression<Func<TasksTimer, bool>> where)
  {
    var rows = db.TasksTimers
      .Where(where)
      .Where(x => x.StaffId == staff_id)
      .Select(x => x.TaskId)
      .Distinct()
      .ToList();

    return rows;
  }

  /**
   * @deprecated
   */
  private double _cal_total_logged_array_from_timers(List<TasksTimer> timers)
  {
    var output = timers.Select(x =>
      {
        var _tspent = x.EndTime == null
          ? DateTime.Now - x.StartTime
          : x.EndTime - x.StartTime;
        return _tspent;
      })
      .ToList()
      .Select(x => x!.Value.TotalSeconds)
      .Sum();


    return output;
  }

  public bool delete_timesheet(int id)
  {
    var timesheet = db.TasksTimers.FirstOrDefault(x => x.Id == id);
    var result = db.TasksTimers.Where(x => x.Id == id).Delete();
    if (result <= 0) return false;
    {
      db.Taggables.Where(x => x.RelId == id && x.RelType == "timesheet").Delete();
      db.SaveChanges();

      var task = db.Tasks.FirstOrDefault(x => x.Id == timesheet.TaskId);

      if (task.RelType == "project")
      {
        var additional_data = task.Name;
        var total = timesheet.EndTime - timesheet.StartTime;
        additional_data += "<br /><seconds>{total}</seconds>";
        projects_model.log(
          task.RelId!.Value,
          "project_activity_task_timesheet_deleted",
          additional_data,
          task.VisibleToClient
        );
      }

      hooks.do_action("task_timer_deleted", timesheet);
      log_activity($"Timesheet Deleted [ {id} ]");

      return true;
    }
  }

  public List<Reminder> get_reminders(int task_id)
  {
    var rows = db.Reminders
      .Where(x => x.RelId == task_id && x.RelType == "task")
      .OrderBy(x => x.IsNotified)
      .ThenBy(x => x.Date)
      .ToList();
    return rows;
  }

  /**
   * Check whether the given staff can access the given task
   *
   * @param  int staff_id
   * @param  int task_id
   *
   * @return boolean
   */
  public bool can_staff_access_task(int staff_id, int task_id)
  {
    var retVal = false;
    var staffCanAccessTasks = get_staff_members_that_can_access_task(task_id);
    staffCanAccessTasks.ForEach(staff =>
    {
      if (staff.Id == staff_id) retVal = true;
    });
    return retVal;
  }

  /**
   * Get the staff members that can view the given task
   *
   * @param  int taskId
   *
   * @return array
   */
  public List<Staff> get_staff_members_that_can_access_task(int taskId)
  {
    var rows = db.Staff
      .Where(x =>
        x.IsAdmin ||
        db.TaskAssigneds.Where(y => y.TaskId == taskId).Select(y => y.StaffId).Contains(x.Id) ||
        db.TaskFollowers.Where(y => y.TaskId == taskId).Select(y => y.StaffId).Contains(x.Id) ||
        db.Tasks.Where(y => y.Id == taskId && !y.IsAddedFromContact).Select(y => y.AddedFrom).Contains(x.Id) ||
        db.StaffPermissions.Where(y => y.Feature == "tasks" && y.Capability == "view").Select(y => y.StaffId).Contains(x.Id)
      )
      .ToList()
      .Where(x => x.Active.Value)
      .ToList();

    return rows;
  }

  /**
   * Check whether the given staff should receive notification for
   * the given task
   *
   * @param  int staffid
   * @param  int taskid  [description]
   *
   * @return boolean
   */
  private bool should_staff_receive_notification(int staffid, int taskid)
  {
    if (!can_staff_access_task(staffid, taskid)) return false;

    return hooks.apply_filters("should_staff_receive_task_notification",
      is_task_assignee(staffid, taskid) ||
      is_task_follower(staffid, taskid) ||
      is_task_creator(staffid, taskid) ||
      staff_has_commented_on_task(staffid, taskid)
      // ['staff_id' => staffid, 'task_id' => taskid]
    );
  }

  /**
   * Send notifications on new task comment
   *
   * @param  string description
   * @param  int taskid
   * @param  array staff
   * @param  string email_template
   * @param  array notification_data
   * @param  int comment_id
   *
   * @return void
   */
  private async void _send_task_mentioned_users_notification(string description, int taskid, List<int> staff, string email_template, string notification_data, int? comment_id = null)
  {
    // staff = array_unique(staff, SORT_NUMERIC);
    var notifiedUsers = new List<int>();

    foreach (var staffId in staff)
    {
      if (!db.is_client_logged_in())
        if (staffId == staff_user_id)
          continue;
      var member = staff_model.get(x => x.Id == staffId).First();
      var link = $"#taskid={taskid}";
      if (comment_id.HasValue)
        link += $"#comment_{comment_id}";

      var notified = db.add_notification(new Notification
      {
        Description = description,
        ToUserId = member.Id,
        Link = link,
        AdditionalData = notification_data
      });


      if (notified != null) notifiedUsers.Add(member.Id);

      if (email_template != "") self.helper.send_mail_template(email_template, member.Email, member.Id, taskid);
    }

    db.pusher_trigger_notification(notifiedUsers);
  }

  public void update_checklist_assigned_staff(TaskChecklistItem data)
  {
    var assigned = data.Assigned;
    if (assigned is null or 0) assigned = null;
    var result = db.TaskChecklistItems
      .Where(x => x.Id == data.Id)
      .Update(x => new TaskChecklistItem { Assigned = assigned });

    if (result > 0) return;
  }

  public void do_kanban_query(int status, string search = "", int page = 1, bool count = false, Expression<Func<Task, bool>> where = default)
  {
    //_deprecated_function('Tasks_model::do_kanban_query', '2.9.2', 'TasksKanban class');

    // var kanBan = new TasksKanban(status)
    //   .search(search)
    //   .page(page)
    //   .sortBy(sort['sort'] ?? null, sort['sort_by'] ?? null);
    //
    // // if (where) {
    // //     kanBan.tapQuery(  (status, ci)=> use (where) {
    // //         ci.db.where(where);
    // //     });
    // // }
    //
    // if (count) return kanBan.countAll();
    //
    // return kanBan.get();
  }
}
