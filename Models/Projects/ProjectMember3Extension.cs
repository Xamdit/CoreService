using Microsoft.EntityFrameworkCore;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Task = Service.Entities.Task;

namespace Service.Models.Projects;

public static class ProjectMember3Extension
{
  public static List<ProjectSetting> get_project_settings(this ProjectsModel model, int project_id)
  {
    var (self, db) = model.getInstance();
    var rows = db.ProjectSettings
      .Where(x => x.ProjectId == project_id)
      .ToList();
    return rows;
  }

  public static List<ProjectMember> get_project_members(this ProjectsModel model, int id, bool with_name = false)
  {
    var (self, db) = model.getInstance();
    var query = db.ProjectMembers.Include(x => x.Staff).Where(x => x.ProjectId == id).AsQueryable();
    return query.ToList();
  }

  public static bool remove_team_member(this ProjectsModel model, int project_id, int staff_id)
  {
    var (self, db) = model.getInstance();
    var affected_rows = db.ProjectMembers.Where(x => x.ProjectId == project_id && x.StaffId == staff_id).Delete();
    if (affected_rows <= 0) return false;

    // Remove member from tasks where is assigned
    db.TaskAssigneds
      .Where(x =>
        x.StaffId == staff_id &&
        db.Tasks.Any(y => y.Id == x.TaskId && y.RelType == "project" && y.RelId == project_id)
      )
      .Delete();

    model.log(project_id, "project_activity_removed_team_member", db.get_staff_full_name(staff_id));
    return true;
  }


  public static async Task<List<TasksTimer>> get_timesheets(this ProjectsModel model, int project_id, params int[] tasks_ids)
  {
    return await model.get_timesheets(project_id, tasks_ids.ToList());
  }

//
  public static async Task<List<TasksTimer>> get_timesheets(this ProjectsModel model, int project_id, List<int> tasks_ids)
  {
    var (self, db) = model.getInstance();
    var tasks_model = self.tasks_model(db);
    if (tasks_ids.Any())
    {
      var condition = CreateCondition<Task>(x => x.Id == project_id);
      var tasks = await model.get_tasks(condition);
      tasks_ids = tasks.Select(x => x.Id).ToList();
    }

    if (tasks_ids.Any()) return new List<TasksTimer>();

    var timesheets = db.TasksTimers
      .Include(x => x.Staff)
      .Where(x => tasks_ids.Contains(x.TaskId))
      .ToList()
      .Select(t =>
      {
        var task = tasks_model.get(x => x.Id == t.TaskId);
        t.Task = task;
        t.Staff.FirstName = db.get_staff_full_name(t.StaffId);
        // if (!is_null(t.EndTime))
        //   t.TotalSpent                 = t.EndTime - t.StartTime;
        // else
        //   t.TotalSpent                 = time() - t.StartTime;
        return t;
      })
      .ToList();

    return timesheets;
  }

//
  public static ProjectDiscussion? get_discussion(this ProjectsModel model, int id, int? project_id = null)
  {
    var (self, db) = model.getInstance();
    var query = db.ProjectDiscussions
      .Include(x => x.ProjectDiscussionComments)
      .Where(x => x.Id == id).AsQueryable();
    if (project_id.HasValue) query.Where(x => x.ProjectId == project_id);
    if (db.client_logged_in())
      query = query.Where(x =>
        x.ShowToCustomer &&
        db.Projects.Any(y => y.ClientId == model.client_user_id)
      );
    var discussion = query.FirstOrDefault();
    return discussion ?? null;
  }

//
  public static ProjectDiscussionComment get_discussion_comment(this ProjectsModel model, int id)
  {
    var (self, db) = model.getInstance();
    var comment = new DataSet<ProjectDiscussionComment>();
    // comment.Data = db.ProjectDiscussionComments.FirstOrDefault(x => x.Id == id);
    comment.Data = db.ProjectDiscussionComments.Find(id)!;
    if (comment.Data.ContactId != 0)
    {
      comment.created_by_current_user = db.client_logged_in() && comment.Data.ContactId == db.get_contact_user_id();
      var profile_picture_url = db.contact_profile_image_url(comment.Data.ContactId);
      comment.profile_picture_url = profile_picture_url;
    }
    else
    {
      comment.created_by_current_user = db.client_logged_in()
        ? false
        : comment.created_by_current_user = db.is_staff_logged_in()
          ? comment.staff_id == model.staff_user_id
          : false;

      // comment.created_by_admin = helper.is_admin(comment.staff_id);
      comment.created_by_admin = db.is_admin(comment.Data.StaffId);
      comment.Data.profile_picture_url = staff_profile_image_url(comment.Data.StaffId);
    }

    comment.Data.DateCreated *= 1000;
    if (!string.IsNullOrEmpty(comment.Modified))
      comment.Data.Modified = DateTime.Parse(comment.Data.Modified) * 1000;
    if (!string.IsNullOrEmpty(comment.Data.FileName))
      comment.file_url = site_url($"uploads/discussions/{comment.Data.DiscussionId}/{comment.Data.FileName}");

    return comment;
  }

//
  public static List<DataSet<ProjectDiscussionComment>> get_discussion_comments(this ProjectsModel model, int id, string type)
  {
    var (self, db) = model.getInstance();
    var rows = db.ProjectDiscussionComments.Where(x => x.DiscussionId == id && x.DiscussionType == type).ToList();
    // var comments = converts<dynamic>(rows);
    var comments = rows.Select(x =>
    {
      var output = new DataSet<ProjectDiscussionComment>();
      output.Data = x;
      return output;
    }).ToList();


    var allCommentsIDS = comments.Select(x => x.Data?.Id).ToList();
    var allCommentsParentIDS = comments.Select(x => x.Data?.Parent).ToList();
    var output = comments.Select(comment =>
      {
        if (comment.Data.ContactId != 0)
        {
          comment.Options.Add(new Option()
          {
            Name = "created_by_current_user",
            Value = $"{db.client_logged_in() && comment.Data.ContactId == db.get_contact_user_id()}"
          });
          comment.Options.Add(new Option()
          {
            Name = "profile_picture_url",
            Value = $"{db.contact_profile_image_url(comment.Data.ContactId)}"
          });
        }
        else
        {
          var value = !db.client_logged_in() && db.is_staff_logged_in() && comment.Data.StaffId == model.staff_user_id;
          comment.Options.Add(new Option()
          {
            Name = "created_by_current_user",
            Value = $"{value}"
          });
          comment.Options.Add(new Option()
          {
            Name = "created_by_admin",
            Value = $"{comment.Data.StaffId.is_admin()}"
          });
          comment.Options.Add(new Option()
          {
            Name = "profile_picture_url",
            Value = $"{staff_profile_image_url(comment.Data.StaffId)}"
          });
        }

        // if (!string.IsNullOrEmpty(comment.FileName)) comment.file_url = helper.site_url($"uploads/discussions/{id}/{comment.FileName}");

        // item.Data.DateCreated *= 1000;
        // if (!string.IsNullOrEmpty(comment.Modified))
        //   item.Data.Modified *= 1000;
        return comment;
      })
      .ToList();


    // Ticket #5471

    return allCommentsParentIDS.Where(parent_id => !allCommentsIDS.Contains(parent_id.Value))
      .Aggregate(comments, (current, parent_id) => current.Select(comment =>
        {
          var key = comment.Data.FileName;
          if (comment.Data.Parent == parent_id) comment.Data.Parent = null;
          return comment;
        })
        .ToList());
  }

//
  public static List<DataSet<ProjectDiscussion>> get_discussions(this ProjectsModel model, int project_id)
  {
    var (self, db) = model.getInstance();
    var query = db.ProjectDiscussions.Where(x => x.ProjectId == project_id).AsQueryable();

    if (db.client_logged_in())
      query.Where(x => x.ShowToCustomer);
    var discussions = query.ToList();
    var output = discussions.Select(discussion =>
      {
        var item = new DataSet<ProjectDiscussion>();
        item.Data = discussion;
        var count = db.ProjectDiscussionComments.Count(x => x.DiscussionId == discussion.Id && x.DiscussionType == "regular");
        item.Options.Add(new Option()
        {
          Name = "total_comments",
          Value = $"{count}"
        });
        // discussion.total_comments = db.ProjectDiscussionComments.Count(x => x.DiscussionId == discussion.Id && x.DiscussionType == "regular");
        return item;
      })
      .ToList();
    return output;
  }

//
  public static ProjectDiscussionComment? add_discussion_comment(this ProjectsModel model, DataSet<ProjectDiscussionComment> data, int discussion_id, string type)
  {
    var (self, db) = model.getInstance();
    var discussion = model.get_discussion(discussion_id);
    var _data = new DataSet<ProjectDiscussionComment>();


    _data.Data.DiscussionId = discussion_id;
    _data.Data.DiscussionType = type;
    if (string.IsNullOrEmpty(data.Data.Content)) _data.Data.Content = data.Data.Content;
    if (data.Data.Parent.HasValue) _data.Data.Parent = data.Data.Parent;
    if (db.client_logged_in())
    {
      _data.Data.ContactId = db.get_contact_user_id();
      // _data.Contact.FullName = helper.get_contact_full_name(_data.ContactId.Value);
      _data.Data.StaffId = 0;
    }
    else
    {
      _data.Data.ContactId = 0;
      _data.Data.StaffId = model.staff_user_id;
      _data.Data.FullName = db.get_staff_full_name(_data.Data.StaffId);
    }

    _data = handle_project_discussion_comment_attachments(discussion_id, data, _data);
    _data.Data.DateCreated = DateTime.Now;
    _data = hooks.apply_filters("before_add_project_discussion_comment", _data, discussion_id);

    var result = db.ProjectDiscussionComments.Add(_data.Data);
    var insert_id = result.Entity.Id;
    if (!result.IsAdded()) return null;
    var not_link = "#";
    if (type == "regular")
    {
      discussion = model.get_discussion(discussion_id);
      not_link = $"projects/view/{discussion.ProjectId}?group=project_discussions&discussion_id={discussion_id}";
    }
    else
    {
      var misc_model = self.misc_model(db);
      var row = misc_model.get_file(discussion_id);
      discussion = convert<ProjectDiscussion>(row);
      not_link = $"projects/view/{discussion.ProjectId}?group=project_files&file_id={discussion_id}";
    }

    var emailTemplateData = new
    {
      staff = new
      {
        discussion_id,
        discussion_comment_id = insert_id,
        discussion_type = type
      },
      customers = new
      {
        customer_template = true,
        discussion_id,
        discussion_comment_id = insert_id,
        discussion_type = type
      }
    };

    // if (!string.IsNullOrEmpty(_data.FileName))
    //   emailTemplateData.attachments = new List<EmailTemplate>
    //   {
    //     new
    //     {
    //       Attachment = globals<string>("PROJECT_DISCUSSION_ATTACHMENT_FOLDER") + discussion_id + $"/{_data.FileName}",
    //       filename = _data.FileName,
    //       type = _data.FileMimeType,
    //       read = true
    //     }
    //   };

    var notification_data = new Notification
    {
      Description = "not_commented_on_project_discussion",
      Link = not_link
    };

    if (db.client_logged_in())
      notification_data.FromClientId = db.get_contact_user_id();
    else
      notification_data.FromUserId = model.staff_user_id;
    var notifiedUsers = new List<int>();

    var regex = "/data-mention-id='(d+)\'/";
    var matcher = preg_match_all(regex, data.Data.Content);
    if (matcher.Any())
    {
      var members = matcher.First().Select(int.Parse).Distinct().OrderBy(x => x).ToList();
      model.send_project_email_mentioned_users(discussion.ProjectId, "project_new_discussion_comment_to_staff", members.Select(x => db.staff(x)).ToList(), emailTemplateData);
      members.Select(memberId =>
        {
          if (memberId == model.staff_user_id && !db.client_logged_in()) return 0;
          notification_data.ToUserId = memberId;
          return db.add_notification(notification_data) ? memberId : 0;
        })
        .ToList()
        .Where(x => x > 0)
        .ToList();
    }
    else
    {
      model.send_project_email_template(
        discussion.ProjectId,
        "project_new_discussion_comment_to_staff",
        "project_new_discussion_comment_to_customer",
        discussion.ShowToCustomer,
        emailTemplateData
      );
      model.get_project_members(discussion.ProjectId)
        .Where(member => member.StaffId != model.staff_user_id || db.client_logged_in())
        .ToList()
        .ForEach(member =>
        {
          notification_data.ToUserId = member.StaffId;
          if (db.add_notification(notification_data)) notifiedUsers.Add(member.StaffId);
        });


      // log_activity(
      //   discussion.ProjectId,
      //   "project_activity_commented_on_discussion",
      //   discussion.Subject,
      //   discussion.ShowToCustomer
      // );

      db.pusher_trigger_notification(notifiedUsers);

      model.update_discussion_last_activity(discussion_id, type);

      hooks.do_action("after_add_discussion_comment", insert_id);
    }

    return model.get_discussion_comment(insert_id);
  }

//
  public static ProjectDiscussionComment update_discussion_comment(this ProjectsModel model, ProjectDiscussionComment data)
  {
    var (self, db) = model.getInstance();
    var comment = model.get_discussion_comment(data.Id);

    var affected_rows = db.ProjectDiscussionComments
      .Where(x => x.Id == data.Id)
      .Update(x => new ProjectDiscussionComment
      {
        Modified = today(),
        Content = data.Content
      });
    if (affected_rows > 0) model.update_discussion_last_activity(comment.Id, comment.DiscussionType);
    return model.get_discussion_comment(data.Id);
  }

//
  public static bool delete_discussion_comment(this ProjectsModel model, int id, bool logActivity = true)
  {
    var (self, db) = model.getInstance();
    var comment = model.get_discussion_comment(id);
    var affected_rows = db.ProjectDiscussionComments.Where(x => x.Id == id).Delete();
    if (affected_rows > 0)
    {
      model.delete_discussion_comment_attachment(comment.FileName, comment.DiscussionId);
      if (logActivity)
      {
        var not = "project_activity_deleted_file_discussion_comment";
        // var discussion = model.get_file(comment.DiscussionId);
        var discussion = new ProjectDiscussion();
        var additional_data = $"{discussion.Subject}<br />{comment.Content}";
        if (comment.DiscussionType == "regular")
        {
          discussion = model.get_discussion(comment.DiscussionId);
          not = "project_activity_deleted_discussion_comment";
          additional_data += $"{discussion.Subject}<br />{comment.Content}";
        }

        if (!string.IsNullOrEmpty(comment.FileName)) additional_data += comment.FileName;

        log_activity(discussion.ProjectId, not, additional_data);
      }
    }


    db.ProjectDiscussionComments
      .Where(x => x.Parent == id)
      .Update(x => new ProjectDiscussionComment
      {
        Parent = null
      });
    if (affected_rows > 0 && logActivity) model.update_discussion_last_activity(comment.DiscussionId, comment.DiscussionType);

    return true;
  }

//
  public static void delete_discussion_comment_attachment(this ProjectsModel model, string file_name, int discussion_id)
  {
    var (self, db) = model.getInstance();
    var path = globals<string>("PROJECT_DISCUSSION_ATTACHMENT_FOLDER") + discussion_id;
    if (!string.IsNullOrEmpty(file_name))
      if (file_exists($"{path}/{file_name}"))
        unlink($"{path}/{file_name}");
    if (!is_dir(path)) return;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = list_files(path);
    if (other_attachments.Any())
      delete_dir(path);
  }

//
  public static bool add_discussion(this ProjectsModel model, ProjectDiscussion data)
  {
    var (self, db) = model.getInstance();
    if (db.client_logged_in())
    {
      data.ContactId = db.get_contact_user_id();
      data.StaffId = 0;
      data.ShowToCustomer = true;
    }
    else
    {
      data.StaffId = model.staff_user_id;
      data.ContactId = 0;
      data.ShowToCustomer = data.ShowToCustomer;
    }

    data.DateCreated = DateTime.Now;
    data.Description = data.Description.nl2br();

    var result = db.ProjectDiscussions.Add(data);
    var insert_id = db.SaveChanges();

    if (!result.IsAdded()) return false;
    var members = model.get_project_members(data.Id);
    var notification_data = new Notification
    {
      Description = "not_created_new_project_discussion",
      Link = $"projects/view/{data.Id}?group=project_discussions&discussion_id={insert_id}"
    };

    if (db.client_logged_in())
      notification_data.FromClientId = db.get_contact_user_id();
    else
      notification_data.FromUserId = db.get_staff_user_id();

    var notifiedUsers = new List<int>();
    foreach (var member in members.Where(member => member.StaffId != db.get_staff_user_id() || db.client_logged_in()))
    {
      notification_data.ToUserId = member.StaffId;
      if (db.add_notification(notification_data)) notifiedUsers.Add(member.StaffId);
    }

    db.pusher_trigger_notification(notifiedUsers);
    model.send_project_email_template(data.Id, "project_discussion_created_to_staff", "project_discussion_created_to_customer", data.ShowToCustomer, new
    {
      staff = new
      {
        // discussion_id = insert_id,
        discussion_type = "regular"
      },
      customers = new
      {
        customer_template = true,
        // discussion_id = insert_id,
        discussion_type = "regular"
      }
    });
    // log_activity(data.Id, "project_activity_created_discussion", data.Subject, data.ShowToCustomer);

    return true;
  }

//
  public static bool edit_discussion(this ProjectsModel model, ProjectDiscussion data)
  {
    var (self, db) = model.getInstance();
    var query = db.ProjectDiscussions.Where(x => x.Id == data.Id).AsQueryable();
    var id = data.Id;
    var result = db.ProjectDiscussions
      .Where(x => x.Id == id)
      .Update(x => new ProjectDiscussion
      {
        Subject = data.Subject,
        Description = data.Description,
        ShowToCustomer = data.ShowToCustomer
      });

    if (result <= 0) return false;
    model.log(data.Id, "project_activity_updated_discussion", data.Subject, data.ShowToCustomer);
    return true;
  }

//
  public static bool delete_discussion(this ProjectsModel model, int id, bool logActivity = true)
  {
    var (self, db) = model.getInstance();
    var discussion = model.get_discussion(id);

    var affected_rows = db.ProjectDiscussions.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    // if (logActivity)
    //   log_activity(
    //     discussion.ProjectId,
    //     "project_activity_deleted_discussion",
    //     discussion.Subject,
    //     discussion.ShowToCustomer
    //   );
    model.delete_discussion_comments(id, "regular");

    return true;
  }

//
  public static async Task<bool> copy(this ProjectsModel model, int project_id, DataSet<Project> data)
  {
    var (self, db) = model.getInstance();
    var id = data.Data.Id;
    var project = new DataSet<Project>()
    {
      Data = model.get(x => x.Id == project_id).First()
    };
    var settings = model.get_project_settings(project_id);
    var fields = db.list_fields<Project>();

    var _new_data = new Project
    {
      // ClientId = data.clientid_copy_project,
      ClientId = data.Data.ClientId,
      StartDate = data.Data.StartDate,
      Status = data.Data.StartDate > DateTime.Now ? 1 : 2,
      Deadline = data.Data.Deadline ?? null,
      Name = !string.IsNullOrEmpty(data.Data.Name) ? data.Data.Name : project.Data.Name,
      ProjectCreated = DateTime.Now,
      AddedFrom = model.staff_user_id,
      DateFinished = null
    };

    // Assign fields using LINQ
    fields.ForEach(field =>
    {
      var fieldValue = project.GetType().GetProperty(field)?.GetValue(project);
      _new_data.GetType().GetProperty(field)?.SetValue(_new_data, fieldValue);
    });

    // Add project
    var result = db.Projects.Add(_new_data);
    if (result.Entity.Id == 0) return false;

    // Save tags
    var taggables = db.get_tags_in(project_id, "project");
    db.handle_tags_save(taggables, id, "project");

    // Save settings
    settings.ForEach(setting => db.ProjectSettings.Add(new ProjectSetting
    {
      ProjectId = id,
      Name = setting.Name,
      Value = setting.Value
    }));

    // Copy tasks
    var tasks = await model.get_tasks(x => x.Id == project_id);
    var added_tasks = tasks
      .Where(task => isset(data, "tasks"))
      .Select(task =>
      {
        var daysDifference = task.StartDate - project.Data.StartDate;
        var newTaskStartDate = _new_data.StartDate?.AddDays(daysDifference.Value.Days) ?? DateTime.Now;
        var copy_project_task_status = data.Options.FirstOrDefault(x => x.Name == "copy_project_task_status");
        var status = copy_project_task_status != null
          ? Convert.ToInt32(copy_project_task_status.Value)
          : 0;
        var difference = task.DueDate.HasValue
          ? task.DueDate.Value - task.StartDate
          : (TimeSpan?)null; // Handle null gracefully
        var merge = new Task()
        {
          RelId = id,
          RelType = "project",
          LastRecurringDate = null,
          StartDate = newTaskStartDate,
          Status = status,
          DueDate = task.DueDate.HasValue
            ? newTaskStartDate.AddDays(difference?.TotalDays ?? 0)
            : null
        };
        var tasks_model = self.tasks_model(db);
        var source = new DataSet<Task>();
        source.Data = db.Tasks.Find(task.Id)!;
        return tasks_model.copy(source, merge);
      })
      .Where(task_id => task_id != null)
      .ToList();

    // Copy milestones
    if (data.Options.Any(x => x.Name == "milestones"))
    {
      var milestones = model.get_milestones(x => x.Id == project_id);
      var added_milestones = milestones.Select((milestone, TotalLoggedTime) =>
      {
        var newMilestoneStartDate = _new_data.StartDate?.AddDays((milestone.Data.StartDate - project.Data.StartDate)?.TotalDays ?? 0);
        var newMilestoneDueDate = milestone.Data.DueDate.HasValue
          ? newMilestoneStartDate.Value.AddDays((milestone.Data.DueDate.Value - milestone.Data.StartDate)?.TotalDays ?? 0)
          : (DateTime?)null;
        _new_data.StartDate = _new_data.StartDate?.AddDays((milestone.Data.StartDate - project.Data.StartDate)?.TotalDays ?? 0);
        var newMilestone = new Milestone
        {
          Name = milestone.Data.Name,
          ProjectId = id,
          MilestoneOrder = milestone.Data.MilestoneOrder,
          DescriptionVisibleToCustomer = milestone.Data.DescriptionVisibleToCustomer,
          Description = milestone.Data.Description,
          StartDate = newMilestoneStartDate,
          DueDate = newMilestoneDueDate,
          DateCreated = DateTime.Now,
          Color = milestone.Data.Color,
          HideFromCustomer = milestone.Data.HideFromCustomer
        };

        db.Milestones.Add(newMilestone);
        return new { oldMilestone = milestone, newMilestone };
      }).ToList();

      // Update tasks with milestones
      tasks
        .Where(t => added_tasks.Any(x => x.Id == t.Id) && t.Milestone != 0)
        .ToList()
        .ForEach(task =>
        {
          var milestone = db.Milestones.FirstOrDefault(x => x.Id == task.Milestone);
          if (milestone == null) return;
          var added_milestone = added_milestones.FirstOrDefault(m => m.oldMilestone.Data.Name == milestone.Name)?.newMilestone;
          if (added_milestone != null)
            db.Tasks
              .Where(x => added_tasks.Any(y => y.Id == x.Id) && x.Milestone == task.Milestone)
              .Update(x => new Task { Milestone = added_milestone.Id });
        });
    }

    // Set milestones to 0 if no milestones exist
    if (!data.Options.Any(x => x.Name == "milestones"))
      added_tasks
        .Select(x => x.Id)
        .ToList()
        .ForEach(taskId =>
        {
          db.Tasks
            .Where(x => x.Id == taskId)
            .Update(x => new Task { Milestone = 0 });
        });

    // Add project members
    if (data.Options.Any(x => x.Name == "members"))
    {
      //
      var temp = await model.add_edit_members(model.get_project_members(project_id).ToList());
      Console.WriteLine($"result : {temp}");
      //
    }

    // Save custom fields
    db.get_custom_fields("projects").ForEach(field =>
    {
      var value = db.get_custom_field_value(project_id, field.Id, "projects", false);
      if (!string.IsNullOrEmpty(value))
        db.CustomFieldsValues.Add(new CustomFieldsValue
        {
          RelId = id,
          FieldId = field.Id,
          FieldTo = "projects",
          Value = value
        });
    });

    // Log project creation and activity
    model.log(id, "project_activity_created");
    log_activity($"Project Copied [ID: {project_id}, NewID: {id}]");

    hooks.do_action("project_copied", new { project_id, new_project_id = id });

    return id > 0;
  }

//
  public static string get_staff_notes(this ProjectsModel model, int project_id)
  {
    var (self, db) = model.getInstance();
    var note = db.ProjectNotes.FirstOrDefault(x => x.ProjectId == project_id && x.StaffId == model.staff_user_id);
    return note?.Content;
  }

//
  public static bool save_note(this ProjectsModel model, ProjectNote data, int project_id)
  {
    var (self, db) = model.getInstance();
    // Check if the note exists for this project;
    var notes = db.ProjectNotes.FirstOrDefault(x => x.ProjectId == project_id && x.StaffId == model.staff_user_id);
    if (notes != null)
    {
      var affected_rows = db.ProjectNotes
        .Where(x => x.Id == notes.Id)
        .Update(x => new ProjectNote
        {
          Content = data.Content
        });
      return affected_rows > 0;
    }

    var result = db.ProjectNotes.Add(new ProjectNote
    {
      StaffId = model.staff_user_id,
      Content = data.Content,
      ProjectId = project_id
    });
    return result.IsAdded();
  }
}
