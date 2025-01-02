using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Service.Helpers.Sale;
using Service.Helpers.Tags;
using Service.Helpers.Tasks;
using Service.Models.Client;
using Service.Models.Tasks;
using Service.Models.Users;
using Task = Service.Entities.Task;

namespace Service.Models.Projects;

public class ProjectsModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private List<ProjectSettingOption> statuses = new();
  private CurrenciesModel currencies_model = self.currencies_model(db);
  private ClientsModel clients_model = self.clients_model(db);
  private TasksModel tasks_model = self.tasks_model(db);
  private ProjectsModel projects_model = self.projects_model(db);
  private StaffModel staff_model = self.staff_model(db);

  private List<string> project_settings = hooks.apply_filters("project_settings", new List<string>
  {
    "available_features",
    "view_tasks",
    "create_tasks",
    "edit_tasks",
    "comment_on_tasks",
    "view_task_comments",
    "view_task_attachments",
    "view_task_checklist_items",
    "upload_on_tasks",
    "view_task_total_logged_time",
    "view_finance_overview",
    "upload_files",
    "open_discussions",
    "ViewMileStone",
    "view_gantt",
    "view_timesheets",
    "view_activity_log",
    "view_team_members",
    "hide_tasks_on_main_tasks_table"
  });

  public List<ProjectSettingOption> get_project_statuses()
  {
    statuses = hooks.apply_filters("before_get_project_statuses", new List<ProjectSettingOption>
    {
      new()
      {
        Id = 1,
        Color = "#475569",
        Name = self.helper.label("project_status_1"),
        Order = 1,
        FilterDefault = true
      },
      new()
      {
        Id = 2,
        Color = "#2563eb",
        Name = self.helper.label("project_status_2"),
        Order = 2,
        FilterDefault = true
      },
      new()
      {
        Id = 3,
        Color = "#f97316",
        Name = self.helper.label("project_status_3"),
        Order = 3,
        FilterDefault = true
      },
      new()
      {
        Id = 4,
        Color = "#16a34a",
        Name = self.helper.label("project_status_4"),
        Order = 100,
        FilterDefault = false
      },
      new()
      {
        Id = 5,
        Color = "#94a3b8",
        Name = self.helper.label("project_status_5"),
        Order = 4,
        FilterDefault = false
      }
    });

    // usort(statuses,   (a, b) =>{
    //     return a["order"] - b["order"];
    // });

    return statuses;
  }

  public List<int> get_distinct_tasks_timesheets_staff(int project_id)
  {
    //return $this->db->query('SELECT DISTINCT staff_id FROM ' . db_prefix() . 'taskstimers LEFT JOIN ' . db_prefix() . 'tasks ON ' . db_prefix() . 'tasks.id = ' . db_prefix() . 'taskstimers.task_id WHERE rel_type="project" AND rel_id=' . $this->db->escape_str($project_id))->result_array();
    var result = db.TasksTimers
      .Include(x => x.Task)
      .Where(x => x.Task.RelId == project_id && x.Task.RelType == "project")
      .Select(x => x.StaffId)
      .Distinct()
      .ToList();
    return result;
  }

  public List<ProjectMember> get_distinct_projects_members()
  {
    //return db.query("SELECT staff_id, firstname, lastname FROM  project_members JOIN  staff ON  staff.staffid= project_members.staff_id GROUP by staff_id order by firstname ASC").result_array();
    var result = db.ProjectMembers
      .Include(x => x.Staff)
      .GroupBy(x => new { x.StaffId, x.Staff.FirstName, x.Staff.LastName }) // Group by staff_id, firstname, and lastname
      .Select(g => g.First()) // Select only the group key (StaffId, FirstName, LastName)
      .OrderBy(x => x.Staff.FirstName) // Order by firstname in ascending order
      .ToList(); // Execute the query and return the result as a list
    return result;
  }

  public (int, int) get_most_used_billing_type()
  {
    var result = db.Projects
      .GroupBy(p => p.BillingType)
      .Select(g => new
      {
        BillingType = g.Key,
        TotalUsage = g.Count()
      })
      .OrderByDescending(g => g.TotalUsage)
      .FirstOrDefault();
    return result == null
      ? (0, 0)
      : (result.BillingType, result.TotalUsage);
  }


  public bool timers_started_for_project(int project_id, Expression<Func<TasksTimer, bool>> condition, Expression<Func<TasksTimer, bool>> task_timers_where)
  {
    var total = db.TasksTimers.Include(x => x.Task)
      .Where(condition)
      .Count(
        x =>
          x.Task.RelId == project_id &&
          x.Task.RelType == "project" &&
          x.EndTime == null
      );


    return total > 0;
  }

  public bool pin_action(int id)
  {
    if (db.PinnedProjects.Any(x => x.StaffId == 0 && x.ProjectId == id))
    {
      db.PinnedProjects.Add(new PinnedProject
      {
        StaffId = staff_user_id,
        ProjectId = id
      });
      return true;
    }

    db.PinnedProjects.Where(x => x.StaffId == staff_user_id && x.ProjectId == id).Delete();

    return true;
  }

  public Currency get_currency(int id)
  {
    var customer_currency = clients_model.get_customer_default_currency(db.get_client_id_by_project_id(id));
    var currency = customer_currency != 0
      ? currencies_model.get(customer_currency)
      : currencies_model.get_base_currency();
    return currency;
  }

  public int calc_progress(int id)
  {
    var project = db.Projects.FirstOrDefault(x => x.Id == id);
    if (project.Status == 4) return 100;

    return project.ProgressFromTasks == 1
      ? calc_progress_by_tasks(id)
      : project.Progress.Value + 0;
  }

  public int calc_progress_by_tasks(int id)
  {
    var total_project_tasks = db.Tasks.Count(x => x.RelId == id && x.RelType == "project");
    var total_finished_tasks = db.Tasks.Count(x => x.RelId == id && x.RelType == "project" && x.Status == 5);

    var percent = 0;
    if (total_finished_tasks >= total_project_tasks)
    {
      percent = 100;
    }
    else
    {
      // if (total_project_tasks != 0)
      //   percent = Convert.ToString(number_format((decimal)total_finished_tasks * 100 / (decimal)total_project_tasks, 2));
    }

    return percent;
  }

  public List<ProjectSetting> get_last_project_settings()
  {
    var last_project = db.Projects
      .Include(x => x.ProjectSettings)
      .OrderByDescending(x => x.Id)
      .FirstOrDefault();

    return last_project != null
      ? last_project.ProjectSettings.ToList()
      : new List<ProjectSetting>();
  }

  public List<string> get_settings()
  {
    return project_settings;
  }

  public List<Project> get(Expression<Func<Project, bool>> condition)
  {
    var projects = db.Projects
      .Include(x => x.ProjectSettings)
      .Include(x => x.Client)
      .Where(condition)
      .ToList();
    if (projects.Count() > 1)
    {
      projects = projects.OrderByDescending(x => x.Id).ToList();
      return projects;
    }

    var project = projects.First();
    //project.shared_vault_entries = clients_model.get_vault_entries(project.ClientId,x=>x.ShareInProjects );
    var shared_vault_entries = clients_model.get_vault_entries(project.ClientId, x => x.ShareInProjects);
    var settings = this.get_project_settings(project.Id);

    // SYNC NEW TABS
    var tabs = self.helper.get_project_tabs_admin();
    var tabs_flatten = new List<object>();
    var settings_available_features = new List<object>();

    var available_features_index = string.Empty;
    // var temp = convert<Dictionary<string, object>>(settings);
    var temp = new Dictionary<string, object>();
    settings.ForEach(x => { temp.Add(x.Name, x.Value); });
    foreach (var kvp in temp)
    {
      var key = kvp.Key;
      var setting = kvp.Value;
      if (key != "available_features") continue;
      available_features_index = key;
      var available_features = JsonConvert.SerializeObject(setting);
      if (!string.IsNullOrEmpty(available_features))
        continue;


      var detail = JsonConvert.DeserializeObject<Dictionary<string, object>>(available_features);
      settings_available_features.AddRange(
        JsonConvert.DeserializeObject<Dictionary<string, object>>(available_features)
          .Keys
      );
    }

    foreach (var tab in tabs)
      if (tab.IsCollapse)
        tabs_flatten.AddRange(tab.Children.Select(d => d.Slug));
      else
        tabs_flatten.Add(tab.Slug);
    if (settings_available_features != tabs_flatten)
      foreach (var tab in tabs_flatten.Where(x => !settings_available_features.Contains(x) && !string.IsNullOrEmpty(available_features_index)))
      {
        var currentAvailableFeaturesSettings = settings.FirstOrDefault(x => x.Name == available_features_index) ?? new ProjectSetting();
        var tmp = JsonConvert.SerializeObject(currentAvailableFeaturesSettings);
        // tmp[tab] = 1;

        db.ProjectSettings
          // .Where(x=>x.Id==current_available_features_settings.Id)
          .Where(x => x.Id == currentAvailableFeaturesSettings.Id)
          .Update(x => new ProjectSetting
          {
            Value = tmp
          });
      }

    // project.settings = new StdClass();

    foreach (var setting in project.ProjectSettings)
    {
      // project.settings.{setting.Name} = setting.Value;
    }

    // project.client_data = new StdClass();
    project.Client = clients_model.get(x => x.Id == project.ClientId).FirstOrDefault();
    project.ClientId = project.Client.Id;
    project = hooks.apply_filters("project_get", project);
    // GLOBALS["project"] = project;
    // return project;

    // var rows = db.Projects.Include(x=>x.Client).OrderByDescending(x=>x.Id).ToList();
    // start new manage data


    // end

    var output = new List<Project>();
    output.Add(project);
    return output;
  }

  public (string hours, double total_money) calculate_total_by_project_hourly_rate(double seconds, double hourly_rate)
  {
    var hours = self.helper.seconds_to_time_format(seconds);
    var @decimal = self.helper.sec2qty(seconds);
    double total_money = 0;
    total_money += @decimal * hourly_rate;
    return (hours, total_money);
  }

  public (double total_money, double total_seconds) calculate_total_by_task_hourly_rate(params Task[] tasks)
  {
    double total_money = 0;
    double _total_seconds = 0;

    foreach (var task in tasks)
    {
      var seconds = task.total_logged_time();
      _total_seconds += seconds;
      total_money += self.helper.sec2qty(seconds) * task.HourlyRate;
    }

    return (total_money, _total_seconds);
  }

  // public async Task<bool> get_tasks(int id,Expression<Func<object,true>> where ,bool apply_restrictions = false,bool count = false,Action callback = null){
  public async Task<List<Task>> get_tasks(Expression<Func<Task, bool>> condition, bool apply_restrictions = false, bool count = false, Action<(int? count, int page, int limit)> action = null)
  {
    var can_view_tasks = db.has_permission("tasks", "", "view");
    var show_all_tasks_for_project_member = db.get_option<bool>("show_all_tasks_for_project_member");
    var query = db.Tasks
      .Include(x => x.Milestone)
      .Where(x => x.RelType == "project")
      .Where(condition)
      .AsQueryable();

    // if (!client_logged_in && is_staff_logged_in())
    //   select += $",(SELECT staffid FROM  task_assigned WHERE taskid= tasks.id AND staffid={staff_user_id}) as current_user_is_assigned";
    if (db.client_logged_in()) query.Where(x => x.VisibleToClient);
    if (!db.client_logged_in() && !can_view_tasks && !show_all_tasks_for_project_member)
      query = query.Where(x =>
        db.TaskAssigneds.Any(a => a.StaffId == staff_user_id && a.TaskId == x.Id) ||
        db.TaskFollowers.Any(f => f.StaffId == staff_user_id && f.TaskId == x.Id) ||
        x.IsPublic ||
        (x.AddedFrom == staff_user_id && !x.IsAddedFromContact)
      );

    // if (where["milestones.hide_from_customer"])
    // {
    //   db.group_start();
    //   query.Where(x => x.HideFromCustomer == 1);
    //   db.where("milestones.hide_from_customer", where["milestones.hide_from_customer"]);
    //   db.or_where("tasks.milestone", 0);
    //   db.group_end();
    //
    // }

    query = query.Where(condition);


    // Milestones kanban order
    // Request is admin/projects/milestones_kanban
    // if ((this.uri.segment(3) == "milestones_kanban") | (this.uri.segment(3) == "milestones_kanban_load_more")) query = query.OrderBy(x=>x.MilestoneOrder);
    // else
    // {
    //   var orderByString = hooks.apply_filters("project_tasks_array_default_order", "FIELD(status, 5), duedate IS NULL ASC, duedate");
    //   query = query.OrderBy(x=>x.orderByString);
    //   db.order_by(orderByString, "", false);
    // }

    // if (action!=null)
    //   action();

    var tasks = query.ToList();

    // tasks = hooks.apply_filters("get_projects_tasks", tasks, new
    // {
    //   project_id = id,
    //   where = where,
    //   count = count
    // });

    return tasks;
  }

  public bool cancel_recurring_tasks(int id)
  {
    var result = db.Tasks.Where(x =>
        x.RelId == id &&
        x.RelType == "project" &&
        x.Recurring == 1 &&
        (x.Cycles != x.TotalCycles || x.Cycles == 0)
      )
      .Update(x =>
        new Task
        {
          RecurringType = null,
          RepeatEvery = string.Empty,
          Cycles = 0,
          Recurring = 0,
          CustomRecurring = 0,
          LastRecurringDate = null
        });
    return result > 0;
  }

  // public bool do_milestones_kanban_query(int milestone_id,int  project_id,int  page = 1,Expression<Func<>> where = [],bool count = false)
  public async Task<List<Task>> do_milestones_kanban_query(int milestone_id, int project_id, int page = 1, Expression<Func<Task, bool>> where = default, bool count = false)
  {
    var query = db.Tasks
      .Include(x => x.Milestone)
      .Where(x => x.RelType == "project")
      .Where(x => x.Milestone == milestone_id)
      .AsQueryable();
    var action = new Action<(int? count, int page, int limit)>(x =>
    {
      if (!x.count.HasValue) return;
      if (x.page > 1)
      {
        var position = (x.page - 1) * x.limit;
        query = query.Take(x.limit).Skip(position);
      }
      else
      {
        query = query.Take(x.limit);
      }
    });
    var limit = db.get_option<int>("tasks_kanban_limit");
    where = where.And(x => x.Id == project_id);
    var tasks = await get_tasks(where, true, count, action);
    return tasks;
  }

  public List<ProjectFile> get_files(int project_id)
  {
    var query = db.ProjectFiles.Where(x => x.ProjectId == project_id).AsQueryable();
    if (db.client_logged_in()) query = query.Where(x => x.VisibleToCustomer);
    return query.ToList();
  }

  public ProjectFile? get_file(int id, int? project_id = null)
  {
    var query = db.ProjectFiles.Where(x => x.Id == id).AsQueryable();
    if (db.client_logged_in()) query = query.Where(x => x.VisibleToCustomer);
    var file = query.FirstOrDefault();
    if (file == null || !project_id.HasValue) return file;
    return file.ProjectId != project_id ? null : file;
  }

  public bool update_file_data(ProjectFile data)
  {
    var result = db.ProjectFiles.Where(x => x.Id == data.Id).Update(x => data);

    return result > 0;
  }

  public bool change_file_visibility(int id, bool visible)
  {
    var affected_rows = db.ProjectFiles
      .Where(x => x.Id == id)
      .Update(x => new ProjectFile
      {
        VisibleToCustomer = visible
      });
    return affected_rows > 0;
  }

  public bool change_activity_visibility(int id, bool visible)
  {
    var affected_rows =
      db.ProjectActivities
        .Where(x => x.Id == id)
        .Update(x => new ProjectActivity
        {
          VisibleToCustomer = visible
        });
    return affected_rows > 0;
  }

  public bool remove_file(int id, bool logActivity = true)
  {
    hooks.do_action("before_remove_project_file", id);
    var file = db.ProjectFiles.FirstOrDefault(x => x.Id == id);
    if (file == null) return false;

    if (string.IsNullOrEmpty(file.External))
    {
      var path = $"{get_upload_path_by_type("project") + file.ProjectId}/";
      var fullPath = path + file.FileName;
      if (file_exists(fullPath))
      {
        unlink(fullPath);
        var fname = file_name(fullPath);
        var fext = self.helper.file_extension(fullPath);
        var thumbPath = $"{path}{fname}_thumb.{fext}";
        if (file_exists(thumbPath))
          unlink(thumbPath);
      }
    }


    db.ProjectFiles.Where(x => x.Id == id).Delete();
    if (logActivity)
      log(
        file.ProjectId,
        "project_activity_project_file_removed",
        file.FileName,
        file.VisibleToCustomer
      );

    // Delete discussion comments
    this.delete_discussion_comments(id, "file");

    if (!is_dir(get_upload_path_by_type("project") + file.ProjectId)) return true;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = list_files(get_upload_path_by_type("project") + file.ProjectId);
    if (other_attachments.Any())
      delete_dir(get_upload_path_by_type("project") + file.ProjectId);
    return true;
  }

  public async Task<int> calc_milestone_logged_time(int project_id, int id)
  {
    var total = new List<object>();
    var condition = CreateCondition<Task>(x => x.Id == project_id && x.Milestone == id);
    var tasks = await get_tasks(condition);
    return tasks.Select(task =>
      {
        // return task.TotalLoggedTime;
        return 0;
      })
      .ToList()
      .Sum();
  }

  public double total_logged_time(int id)
  {
    var totalLoggedTime = db.TasksTimers
      .Include(x => x.Task)
      .Where(x => x.Task.RelType == "project" && x.Task.RelId == id)
      .Select(x =>
        x.EndTime == null
          ? DateTime.Now - x.StartTime
          : x.EndTime - x.StartTime
      )
      .Select(x => x != null ? x.Value.TotalMilliseconds : 0)
      .ToList();

    return totalLoggedTime.Sum();
  }

  // public List<(Milestone milestone, Task<int> TotalLoggedTime)> get_milestones(Expression<Func<Milestone, bool>> condition)
  // {
  //
  //   var project_id = db.ExtractIdFromCondition(condition);

  //   var milestones = db.Milestones.Where(condition)
  //     .Where(x => x.ProjectId == project_id)
  //     .OrderBy(x => x.MilestoneOrder)
  //     .ToList();
  //   var output = milestones.Select(milestone => (milestone, calc_milestone_logged_time(project_id, milestone.Id))).ToList();
  //   return output;
  // }

  public List<DataSet<Milestone>> get_milestones(Expression<Func<Milestone, bool>> condition)
  {
    var project_id = db.ExtractIdFromCondition(condition)!.Value;
    var milestones = db.Milestones.Where(condition)
      .Where(x => x.ProjectId == project_id)
      .OrderBy(x => x.MilestoneOrder)
      .ToList();
    // var output = milestones.Select(milestone => (milestone, calc_milestone_logged_time(project_id, milestone.Id))).ToList();
    var output = milestones.Select(milestone =>
      {
        var item = new DataSet<Milestone>();
        item.Data = milestone;
        item["total_logged_time"] = calc_milestone_logged_time(project_id, milestone.Id);
        return item;
      })
      .ToList();


    return output;
  }


  public bool update_milestone(Milestone data)
  {
    var id = data.Id;
    var milestone = db.Milestones.FirstOrDefault(x => x.Id == id);
    data.Description = data.Description.nl2br();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;

    var project = get(x => x.Id == milestone.ProjectId).FirstOrDefault();
    var show_to_customer = db.view_milestone(project.ProjectSettings);
    log(milestone.ProjectId, "project_activity_updated_milestone", milestone.Name, show_to_customer);
    log_activity($"Project Milestone Updated [ID:{id}]");
    return true;
  }

  public bool update_task_milestone(Task data, params Task[] orders)
  {
    db.Tasks
      .Where(x => x.Id == data.Id)
      .Update(x => new Task
      {
        Milestone = data.Milestone
      });

    orders.ToList()
      .ForEach(order =>
      {
        db.Tasks
          .Where(x => x.Id == order.Id)
          .Update(x => new Task
          {
            MilestoneOrder = order.MilestoneOrder
          });
      });

    return true;
  }

  public bool update_milestones_order(params Milestone[] data)
  {
    data.ToList()
      .ForEach(milestone =>
      {
        db.Milestones
          .Where(x => x.Id == milestone.Id)
          .Update(x => new Milestone
          {
            MilestoneOrder = milestone.MilestoneOrder
          });
      });
    return true;
  }

  public bool update_milestone_color(Milestone data)
  {
    db.Milestones
      .Where(x => x.Id == data.Id)
      .Update(x => new Milestone
      {
        Color = data.Color
      });
    return true;
  }


  public int add(DataSet<Project> dataset)
  {
    var send_created_email = (bool)dataset["send_created_email"];

    var send_project_marked_as_finished_email_to_contacts = false;
    if (dataset.ContainesKey("project_marked_as_finished_email_to_contacts"))
      send_project_marked_as_finished_email_to_contacts = Convert.ToBoolean(dataset["project_marked_as_finished_email_to_contacts"]);

    if (dataset.Data.ProjectSettings.Any())
      project_settings = convert<Dictionary<string, object>>(dataset.Data.ProjectSettings).Keys.ToList();
    var custom_fields = new List<CustomField>();
    if (dataset.custom_fields.Any()) custom_fields = dataset.custom_fields;

    if (dataset.Data.ContactNotification.HasValue)
      dataset.Data.NotifyContacts = JsonConvert.SerializeObject(dataset.Data.ContactNotification == 2 ? dataset.Data.NotifyContacts : new List<int>());

    dataset.Data.StartDate = dataset.Data.StartDate;

    if (!dataset.Data.Deadline.HasValue)
      dataset.Data.Deadline = dataset.Data.Deadline;
    else dataset.Data.ProjectCreated = DateTime.Now;

    var project_members = new List<ProjectMember>();
    if (dataset.Data.ProjectMembers.Any())
      project_settings = convert<Dictionary<string, object>>(dataset.Data.ProjectMembers).Keys.ToList();

    if (dataset.Data.BillingType == 1)
    {
      dataset.Data.ProjectRatePerHour = 0;
    }
    else if (dataset.Data.BillingType == 2)
    {
      dataset.Data.ProjectCost = false;
    }
    else
    {
      dataset.Data.ProjectRatePerHour = 0;
      dataset.Data.ProjectCost = false;
    }

    dataset.Data.AddedFrom = staff_user_id;
    var items_to_convert = new List<Itemable>();
    int? estimate_id = null;
    var items_assignees = new List<int>();
    if (dataset.items.Any())
    {
      items_to_convert = dataset.items;
      estimate_id = dataset.Data.Estimates.First().Id;
      items_assignees = convert<List<int>>(dataset["items_assignee"]);
    }

    dataset = hooks.apply_filters("before_add_project", dataset);
    List<Taggable> tags = new();
    if (dataset.Tags.Any()) tags = dataset.Tags;


    var result = db.Projects.Add(project(dataset));
    if (!result.IsAdded()) return 0;
    var insert_id = result.Entity.Id;
    db.handle_tags_save(tags, insert_id, "project");
    if (custom_fields.Any())
      self.helper.handle_custom_fields_post(insert_id, custom_fields);
    // var _pm = this;
    //   if (project_members.Any())
    //   {
    //     _pm.ProjectMembers = project_members;
    //     this.add_edit_members(_pm, insert_id);
    //   }

    var original_settings = get_settings();
    var value_setting = string.Empty;
    if (project_settings.Any())
    {
      var _settings = new List<string>();
      var _values = new Dictionary<string, List<string>>();

      project_settings.ForEach(project_setting =>
      {
        var value = new List<string>();
        var name = project_setting;
        _settings.Add(name);
        _values.Add(name, value);
      });


      foreach (var setting in original_settings)
      {
        if (setting != "available_features")
        {
          value_setting = _settings.Contains(setting) ? "1" : "0";
        }
        else
        {
          var tabs = self.helper.get_project_tabs_admin();
          var tab_settings = new Dictionary<string, bool>();
          foreach (var tab in _values[setting])
            tab_settings[tab] = true;
          foreach (var tab in tabs)
            if (!tab.IsCollapse)
            {
              if (!_values[setting].Contains(tab.Slug))
                tab_settings[tab.Slug] = false;
            }
            else
            {
              foreach (var tab_dropdown in tab.Children.Where(tab_dropdown => !_values[setting].Contains(tab_dropdown.Slug)))
                tab_settings[tab_dropdown.Slug] = false;
            }

          value_setting = JsonConvert.SerializeObject(tab_settings);
        }

        db.ProjectSettings.Add(new ProjectSetting
        {
          ProjectId = insert_id,
          Name = setting,
          Value = value_setting
        });
      }
    }
    else
    {
      foreach (var setting in original_settings)
        //var value_setting = 0;
        db.ProjectSettings.Add(new ProjectSetting
        {
          ProjectId = insert_id,
          Name = setting,
          Value = "0"
        });
    }

    if (items_to_convert.Any() && estimate_id > 0)
    {
      this.convert_estimate_items_to_tasks(insert_id, items_to_convert, items_assignees, dataset, project_settings);
      db.Estimates
        .Where(x => x.Id == estimate_id)
        .Update(x => new Estimate
        {
          ProjectId = insert_id
        });
    }

    log_activity(insert_id, "project_activity_created");
    if (send_created_email) this.send_project_customer_email(insert_id, "project_created_to_customer");

    if (send_project_marked_as_finished_email_to_contacts) this.send_project_customer_email(insert_id, "project_marked_as_finished_to_customer");

    hooks.do_action("after_add_project", insert_id);

    log_activity($"New Project Created [ID: {insert_id}]");

    return insert_id;
  }

  public bool update(DataSet<Project> dataset)
  {
    var id = dataset.Data.Id;
    var row = db.Projects.FirstOrDefault(x => x.Id == dataset.Data.Id);
    var old_status = row.Status;
    var send_created_email = (bool)dataset["send_created_email"];
    var send_project_marked_as_finished_email_to_contacts = (bool)dataset["project_marked_as_finished_email_to_contacts"];
    var original_project = get(x => x.Id == id).FirstOrDefault();
    var notify_project_members_status_change = (bool)dataset["notify_project_members_status_change"];

    var affectedRows = 0;
    if (dataset.Data.ProjectSettings.Any())
    {
      var result = db.ProjectSettings.Where(x => x.ProjectId == id)
        .Update(x => new ProjectSetting
        {
          Value = null
        });
      if (result > 0) affectedRows++;
    }
    else
    {
      var _settings = new List<object>();
      var _values = new Dictionary<string, List<string>>();

      var projectSettings = dataset.Data.ProjectSettings;

      foreach (var kvp in projectSettings)
      {
        var name = kvp.Name;
        var val = kvp.Value;
        _settings.Add(name);
        _values[name] = new List<string>() { val };
      }

      dataset.Data.ProjectSettings.Clear();
      var original_settings = this.get_project_settings(id);
      var value_setting = false;
      foreach (var setting in original_settings)
      {
        if (setting.Name != "available_features")
        {
          value_setting = _settings.Contains(setting.Name);
        }
        else
        {
          var tabs = self.helper.get_project_tabs_admin();
          var tab_settings = new Dictionary<string, bool>();
          foreach (var tab in _values[setting.Name])
            tab_settings[tab] = true;
          foreach (var tab in tabs)
            if (!tab.IsCollapse)
            {
              if (!_values[setting.Name].Contains(tab.Slug))
                tab_settings[tab.Slug] = false;
            }
            else
            {
              foreach (var tab_dropdown in tab.Children.Where(tab_dropdown => _values[setting.Name].Contains(tab_dropdown.Slug)))
                tab_settings[tab_dropdown.Slug] = false;
            }

          value_setting = tab_settings.Values.Any();
        }

        var result = db.ProjectSettings
          .Where(x => x.ProjectId == id && x.Name == setting.Name)
          .Update(x => new ProjectSetting
          {
            Value = value_setting ? "true" : "false"
          });
        if (result > 0) affectedRows++;
      }
    }

    if (old_status == 4 && dataset.Data.Status != 4)
      dataset.Data.DateFinished = null;
    else if (dataset.Data.DateFinished.HasValue)
      dataset.Data.DateFinished = dataset.Data.DateFinished;
    if (dataset.custom_fields.Any())
    {
      var custom_fields = dataset.custom_fields;
      if (self.helper.handle_custom_fields_post(id, custom_fields)) affectedRows++;
    }

    dataset.Data.StartDate = dataset.Data.StartDate;
    switch (dataset.Data.BillingType)
    {
      case 1:
        dataset.Data.ProjectRatePerHour = 0;
        break;
      case 2:
        dataset.Data.ProjectCost = false;
        break;
      default:
        dataset.Data.ProjectRatePerHour = 0;
        dataset.Data.ProjectCost = false;
        break;
    }

    var project_members = new List<ProjectMember>();
    if (!dataset.Data.ProjectMembers.Any())
      project_members = dataset.Data.ProjectMembers.ToList();

    var _pm = new List<ProjectMember>();
    if (project_members.Any())
      _pm = project_members;
    // if (this.add_edit_members(_pm.First(), id))
    //   affectedRows++;
    var mark_all_tasks_as_completed = false;
    // if (mark_all_tasks_as_completed)
    //   mark_all_tasks_as_completed = true;
    if (dataset.Tags.Any())
      if (db.handle_tags_save(dataset.Tags, id, "project"))
        affectedRows++;
    if (dataset.Options.Any(x => x.Name == "cancel_recurring_tasks"))
      cancel_recurring_tasks(id);
    if (dataset.Data.ContactNotification.HasValue)
      dataset.Data.NotifyContacts = JsonConvert.SerializeObject(dataset.Data.ContactNotification == 2 ? dataset.Data.NotifyContacts : "");

    dataset.Data = hooks.apply_filters("before_update_project", dataset, id);


    var affected_rows = db.Projects.Where(x => x.Id == id).Update(x => dataset.Data);
    if (affected_rows > 0)
    {
      if (mark_all_tasks_as_completed)
        this.mark_all_project_tasks_as_completed(id);
      affectedRows++;
    }

    if (send_created_email)
      if (this.send_project_customer_email(id, "project_created_to_customer"))
        affectedRows++;

    if (send_project_marked_as_finished_email_to_contacts)
      if (this.send_project_customer_email(id, "project_marked_as_finished_to_customer"))
        affectedRows++;
    if (affectedRows <= 0) return false;
    log_activity(id, "project_activity_updated");
    log_activity($"Project Updated [ID: {id}]");
    if (original_project.Status != dataset.Data.Status)
    {
      hooks.do_action("project_status_changed", new
      {
        status = dataset.Data.Status,
        project_id = id
      });
      // Give space this log to be on top
      Thread.Sleep(100);
      if (dataset.Data.Status == 4)
      {
        log_activity(id, "project_marked_as_finished");
        db.Projects.Where(x => x.Id == id).Update(x => new Project
        {
          DateFinished = DateTime.Now
        });
      }
      else
      {
        log_activity(id, "project_status_updated", $"<b><lang>project_status_{dataset.Data.Status}</lang></b>");
      }

      if (notify_project_members_status_change)
        this.notify_project_members_status_change(id, original_project.Status, dataset.Data.Status);
    }

    hooks.do_action("after_update_project", id);

    return true;
  }


  public bool log(int projectId, string descriptionKey, string additionalData = "", bool visibleToCustomer = true)
  {
    var activity = new ProjectActivity();

    // Check if the request is coming from a cron job or a logged-in user (staff or client)
    if (!is_cron())
    {
      if (db.is_client_logged_in())
      {
        activity.ContactId = db.get_contact_user_id();
        activity.StaffId = 0;
        activity.FullName = self.helper.get_contact_full_name(activity.ContactId);
      }
      else if (db.is_staff_logged_in())
      {
        activity.ContactId = 0;
        activity.StaffId = staff_user_id;
        activity.FullName = db.get_staff_full_name(activity.StaffId);
      }
    }
    else
    {
      activity.ContactId = 0;
      activity.StaffId = 0;
      activity.FullName = "[CRON]";
    }

    // Setting up activity details
    activity.DescriptionKey = descriptionKey;
    activity.AdditionalData = additionalData;
    activity.VisibleToCustomer = visibleToCustomer;
    activity.ProjectId = projectId;
    activity.DateCreated = DateTime.UtcNow;

    // Optional: If you want to modify data before saving

    activity = hooks.apply_filters("before_log_project_activity", activity);

    // Insert the activity into the database using Entity Framework
    db.ProjectActivities.Add(activity);
    return true;
  }

  public bool delete_discussion(int id, bool logActivity = true)
  {
    var discussion = this.get_discussion(id);

    db.ProjectDiscussions.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();

    if (affected_rows <= 0) return false;
    // if (logActivity)
    //   self.helper.log_activity(discussion.ProjectId ?? 0, "project_activity_deleted_discussion", discussion.Subject, discussion.ShowToCustomer);
    this.delete_discussion_comments(id, "regular");
    return true;
  }

  private void notify_project_members_status_change(int id, int old_status, int new_status)
  {
    var members = this.get_project_members(id);
    //var notifiedUsers = new List<int>();
    var notifiedUsers = members.Select(member =>
      {
        if (member.Id != staff_user_id)
        {
          var notified = db.add_notification(new Notification()
          {
            FromUserId = staff_user_id,
            Description = "not_project_status_updated",
            Link = "projects/view/" + id,
            ToUserId = member.Id,
            AdditionalData = JsonConvert.SerializeObject(new List<string>()
            {
              "<lang>project_status_" + old_status + "</lang>",
              "<lang>project_status_" + new_status + "</lang>"
            })
          });
          if (notified) return member.Id;
        }

        return 0;
      })
      .ToList();


    db.pusher_trigger_notification(notifiedUsers);
  }
}
