using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Task = System.Threading.Tasks.Task;

namespace Service.Models.Projects;

public static class ProjectModelExtension
{
  public static int add_milestone(this ProjectsModel model, Milestone data)
  {
    var (self, db) = getInstance();
    data.DateCreated = DateTime.Now;
    data.Description = data.Description.nl2br();
    var result = db.Milestones.Add(data);
    if (!result.IsAdded()) return 0;
    var milestone = db.Milestones.FirstOrDefault(x => x.Id == result.Entity.Id);
    if (milestone == null) return 0;
    var project = model.get(x => x.Id == milestone.ProjectId).FirstOrDefault();
    if (project == null) return 0;
    var show_to_customer = db.view_milestone(project.ProjectSettings);
    model.log(milestone.ProjectId, "project_activity_created_milestone", milestone.Name, show_to_customer);
    log_activity($"Project Milestone Created [ID:{result.Entity.Id}]");
    return result.Entity.Id;
  }

  public static bool delete_milestone(this ProjectsModel model, int id)
  {
    var (self, db) = getInstance();
    var milestone = db.Milestones.FirstOrDefault(x => x.Id == id);
    if (milestone == null) return false;
    var affected_rows = db.Milestones.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;

    var project = model.get(x => x.Id == milestone.ProjectId).First();
    var show_to_customer = db.view_milestone(project.ProjectSettings);
    model.log(milestone.ProjectId, "project_activity_deleted_milestone", milestone.Name, show_to_customer);
    db.Tasks
      .Where(x => x.Milestone == id)
      .Update(x => new Entities.Task { Milestone = null }
      );
    log_activity($"Project Milestone Deleted [{id}]");
    return true;
  }

  /**
 * Simplified bool to send non complicated email templates for project contacts
 * @param  mixed id project id
 * @return boolean
 */
  public static bool send_project_customer_email(this ProjectsModel model, int id, string template)
  {
    var (self, db) = getInstance();
    var sent = false;
    var clients_model = self.model.clients_model();
    var contacts = clients_model.get_contacts_for_project_notifications(id, "project_emails");
    contacts.ForEach(contact =>
    {
      if (self.helper.send_mail_template(template, id, contact.UserId, contact))
        sent = true;
    });


    return sent;
  }

  public static bool mark_as(this ProjectsModel model, Project data)
  {
    var (self, db) = getInstance();
    var row = db.Projects.FirstOrDefault(x => x.Id == data.Id);
    var old_status = row.Status;
    var affected_rows = db.Projects.Where(x => x.Id == data.Id).Update(x => new Project
    {
      Status = data.Status
    });
    if (affected_rows <= 0) return false;

    self.hooks.do_action("project_status_changed", new
    {
      status = data.Status,
      project_id = data.Id
    });


    if (data.Status == 4)
    {
      log_activity(data.Id, "project_marked_as_finished");
      db.Projects
        .Where(x => x.Id == data.Id)
        .Update(x => new Project
        {
          DateFinished = DateTime.Now
        });
    }
    else
    {
      log_activity(data.Id, "project_status_updated", $"<b><lang>project_status_{data.Status}</lang></b>");
      if (old_status == 4)
        db.Projects
          .Where(x => x.Id == data.Id)
          .Update(x => new Project
          {
            DateFinished = null
          });
    }

    // if (data.notify_project_members_status_change == 1)
    //   _notify_project_members_status_change(data.Id, old_status, data.Status);
    //
    // if (data.mark_all_tasks_as_completed == 1)
    //   _mark_all_project_tasks_as_completed(data.Id);
    //
    // if (data.cancel_recurring_tasks && data.cancel_recurring_tasks == "true") cancel_recurring_tasks(data.Id);
    //
    // if (data.send_project_marked_as_finished_email_to_contacts
    //     && data.send_project_marked_as_finished_email_to_contacts == 1
    // )
    model.send_project_customer_email(data.Id, "project_marked_as_finished_to_customer");

    return true;
  }


  public static async Task<bool> _notify_project_members_status_change(this ProjectsModel model, int id, int old_status, int new_status)
  {
    var (self, db) = getInstance();
    var members = model.get_project_members(id);
    // var notifiedUsers = new List<int>();
    var notifiedUsers = members
      .Where(member => member.StaffId != model.staff_user_id)
      .ToList()
      .Select(member =>
      {
        var notified = self.helper.add_notification(new Notification
        {
          FromUserId = model.staff_user_id,
          Description = "not_project_status_updated",
          Link = $"projects/view/{id}",
          ToUserId = member.StaffId,
          AdditionalData = JsonConvert.SerializeObject(new[]
          {
            $"<lang>project_status_{old_status}</lang>",
            $"<lang>project_status_{new_status}</lang>"
          })
        });
        return notified ? member.StaffId : 0;
      })
      .Where(x => x != 0)
      .ToList();

    self.helper.pusher_trigger_notification(notifiedUsers);
    return true;
  }

  public static async Task<ProjectsModel> _mark_all_project_tasks_as_completed(this ProjectsModel model, int id)
  {
    var (self, db) = getInstance();
    await db.Tasks
      .Where(x => x.RelId == id && x.RelType == "project")
      .UpdateAsync(x => new Entities.Task
      {
        Status = 5,
        DateFinished = DateTime.Now
      });
    var tasks = await model.get_tasks(x => x.Id == id);
    tasks.ForEach(task =>
    {
      db.TasksTimers
        .Where(x => x.TaskId == task.Id && !x.EndTime.HasValue)
        .Update(x => new TasksTimer
        {
          EndTime = DateTime.Now
        });
    });

    log_activity(id, "project_activity_marked_all_tasks_as_complete");
    return model;
  }

  public static async Task<bool> add_edit_members(this ProjectsModel model, List<ProjectMember> dataset)
  {
    var items = dataset.Select(model.add_edit_member).ToList();
    var result = (await Task.WhenAll(items)).ToList();
    return result.Any(x => x);
  }

  public static async Task<bool> add_edit_member(this ProjectsModel model, ProjectMember data)
  {
    var (self, db) = getInstance();
    var id = data.Id;
    var affectedRows = 0;

    var project_members = new List<int>();

    var new_project_members_to_receive_email = new List<int>();

    var project = db.Projects.Include(x => x.Client).FirstOrDefault(x => x.Id == id);

    var project_name = project.Name;
    var client_id = project.ClientId;

    var project_members_in = model.get_project_members(id);
    if (project_members_in.Any())
    {
      project_members_in.ForEach(project_member =>
      {
        if (project_members.Contains(project_member.StaffId)) return;
        var affected_rows = db.ProjectMembers.Where(x => x.ProjectId == id && x.StaffId == project_member.StaffId).Delete();
        if (affected_rows <= 0) return;
        db.PinnedProjects.Where(x => x.StaffId == project_member.StaffId && x.ProjectId == id).Delete();
        model.log(id, "project_activity_removed_team_member", self.helper.get_staff_full_name(project_member.StaffId));
        affectedRows++;
      });


      if (project_members.Any())
      {
        var notifiedUsers = project_members.Select(staff_id =>
          {
            var _exists = db.ProjectMembers.Any(x => x.ProjectId == id && x.StaffId == staff_id);
            if (_exists) return 0;
            if (staff_id == 0) return 0;
            var affected_rows = db.ProjectMembers.Add(new ProjectMember
            {
              ProjectId = id,
              StaffId = staff_id
            });
            if (affected_rows.IsAdded()) return 0;
            if (staff_id != model.staff_user_id)
            {
              var notified = self.helper.add_notification(new Notification
              {
                FromUserId = model.staff_user_id,
                Description = "not_staff_added_as_project_member",
                Link = $"projects/view/{id}",
                ToUserId = staff_id,
                AdditionalData = JsonConvert.SerializeObject(new
                {
                  project_name
                })
              });
              new_project_members_to_receive_email.Add(staff_id);
              return notified != null ? staff_id : 0;
            }

            log_activity(id, "project_activity_added_team_member", self.helper.get_staff_full_name(staff_id));
            affectedRows++;
            return 0;
          })
          .ToList();
        self.helper.pusher_trigger_notification(notifiedUsers);
      }
    }
    else
    {
      var notifiedUsers = project_members.Select(staff_id =>
        {
          if (staff_id == 0) return 0;
          var result = db.ProjectMembers.Add(new ProjectMember
          {
            ProjectId = id,
            StaffId = staff_id
          });
          if (!result.IsAdded()) return 0;
          if (staff_id != model.staff_user_id)
          {
            var notified = self.helper.add_notification(new Notification
            {
              FromUserId = model.staff_user_id,
              Description = "not_staff_added_as_project_member",
              Link = $"projects/view/{id}",
              ToUserId = staff_id,
              AdditionalData = JsonConvert.SerializeObject(new
              {
                project_name
              })
            });
            return staff_id;
          }

          model.log(id, "project_activity_added_team_member", self.helper.get_staff_full_name(staff_id));
          return 0;
        })
        .ToList();
      if (notifiedUsers.Any()) self.helper.pusher_trigger_notification(notifiedUsers);
    }

    if (!new_project_members_to_receive_email.Any()) return affectedRows > 0;
    var all_members = model.get_project_members(id);
    all_members.Where(data => new_project_members_to_receive_email.Contains(data.StaffId))
      .ToList()
      .Select(data => self.helper.send_mail_template("project_staff_added_as_member", data, id, client_id))
      .ToList();
    return affectedRows > 0;
  }

  public static bool is_member(this ProjectsModel model, int project_id, int? staff_id = null)
  {
    var (self, db) = getInstance();
    var result = db.ProjectMembers.Any(x => x.ProjectId == project_id && x.StaffId == model.staff_user_id);
    return result;
  }

  public static Project? get_projects_for_ticket(this ProjectsModel model, int client_id)
  {
    var row = model.get(x => x.ClientId == client_id).FirstOrDefault();
    return row;
  }

  public static async Task mark_all_project_tasks_as_completed(this ProjectsModel model, int id)
  {
    var (self, db) = getInstance();
    db.Tasks
      .Where(x => x.RelType == "project" && x.RelId == id)
      .Update(x => new Entities.Task
      {
        // Status = TaskStatus.Complete,
        Status = 5,
        DateFinished = DateTime.Now
      });

    var tasks = await model.get_tasks(x => x.Id == id);
    tasks.ForEach(task =>
    {
      //
      db.TasksTimers.Where(x => x.TaskId == task.Id && x.EndTime == null)
        .Update(x => new TasksTimer()
        {
          EndTime = DateTime.Now
        });
      db.SaveChanges();
      //
    });
    // this.log_activity(id, "project_activity_marked_all_tasks_as_complete");
  }
}
