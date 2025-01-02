using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers.Entities;
using Service.Framework.Helpers.Security;
using Service.Helpers;
using Service.Helpers.Tags;
using Service.Models.Misc;
using Service.Models.Proposals;
using Service.Models.Tasks;
using File = Service.Entities.File;


namespace Service.Models.Leads;

public class LeadsModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private MiscModel misc_model = self.misc_model(db);
  private ProposalsModel proposals_model = self.proposals_model(db);
  private TasksModel tasks_model = self.tasks_model(db);

  public List<(WebToLead formData, List<File> attachments, string publicUrl, Lead lead)> get(Expression<Func<Lead, bool>> condition)
  {
    var id = db.ExtractIdFromCondition(condition);
    var leadResult = db.Leads
      .Where(condition)
      .ToList()
      .Select(lead =>
      {
        var formData = new WebToLead();
        if (lead.FromFormId != 0)
          formData = get_form(x => x.Id == lead.FromFormId);
        var attachments = get_lead_attachments();
        var publicUrl = self.helper.leads_public_url(id!.Value);
        return (formData, attachments, publicUrl, lead);
      })
      .ToList();
    return leadResult;
  }

  // Assuming you have the necessary methods defined in your service for GetForm, GetLeadAttachments, and LeadsPublicUrl
  public Lead? get_lead_by_email(string email)
  {
    var row = db.Leads.Where(x => x.Email == email).FirstOrDefault();
    return row;
  }

  public int add(LeadDto data)
  {
    if (data.option.custom_contact_date != null)
      data.lead.LastContact = today();
    data.lead.Country ??= 0;
    data.lead.Description = data.lead.Description!.Replace("\n", "<br>"); // Converting new lines to <br>
    data.lead.DateAdded = DateTime.Now; // Set the current date and time
    data.lead.AddedFrom = staff_user_id; // Implement this method to get the current user ID
    // Apply hooks before adding lead (implement this method as per your framework)
    data = hooks.apply_filters("before_lead_added", data);
    var tags = new List<Taggable>();
    if (data.Tags != null)
    {
      tags = data.Tags;
      data.Tags = null; // Remove tags from data
    }

    // Handle custom fields
    List<CustomField> customFields = null; // Define your CustomField class
    if (data.CustomFields != null)
    {
      customFields = data.CustomFields;
      data.CustomFields = null; // Remove custom fields from data
    }

    data.lead.Address = data.lead.Address.Trim().Replace("\n", "<br>"); // Trim and convert new lines to <br>
    data.lead.Email = data.lead.Email.Trim(); // Trim the email
    // Insert data into the database
    var result = db.Leads.Add(data.lead); // Assuming Leads is the DbSet<LeadData>
    var insertId = result.Entity.Id; // Assuming Id is the primary key of LeadData
    if (insertId <= 0) return 0; // Return null if the insertion failed
    log_activity($"New Lead Added [ID: {insertId}]");
    log_lead_activity(insertId, "not_lead_activity_created");
    db.handle_tags_save(tags, insertId, "lead"); // Implement this method to handle tags
    if (customFields != null)
      self.helper.handle_custom_fields_post(insertId, customFields); // Implement this method for custom fields
    lead_assigned_member_notification(insertId, data.lead.Assigned); // Implement this method for notifications
    hooks.do_action("lead_created", insertId); // Trigger any hooks related to lead creation
    return insertId;
  }

  public bool update(LeadDto data, int id)
  {
    var current_lead_data = get(x => x.Id == id).First();
    // var current_status = get_status(x=>x.Leads.Where()==current_lead_data.formData).FirstOrDefault();
    var current_status = get_status(x => x.Id == current_lead_data.lead.Status!.Id).FirstOrDefault();
    var current_status_id = 0;
    var current_status_name = string.Empty;
    if (current_status != null)
    {
      current_status_id = current_status.Id;
      current_status_name = current_status.Name;
    }
    else
    {
      current_status_name = new[]
        {
          (current_lead_data.lead.Junk, self.helper.label("lead_junk")),
          (current_lead_data.lead.Lost, self.helper.label("lead_lost"))
        }
        .FirstOrDefault(x => x.Item1).Item2 ?? string.Empty;
    }

    var affectedRows = 0;
    if (data.CustomFields.Any())
    {
      var custom_fields = data.CustomFields;
      if (self.helper.handle_custom_fields_post(id, custom_fields)) affectedRows++;
      data.CustomFields.Clear();
    }

    if (!defined("API"))
    {
      data.lead.Country ??= 0;
      if (string.IsNullOrEmpty(data.lead.Description))
        data.lead.Description = data.lead.Description!.nl2br();
    }

    if (string.IsNullOrEmpty(data.lead.LastContact))
      data.lead.LastContact = null;
    else if (data.lead.LastContact == "0000-00-00 00:00:00")
      data.lead.LastContact = today();
    if (data.Tags.Any())
    {
      if (db.handle_tags_save(data.Tags, id, "lead")) affectedRows++;
      data.Tags.Clear();
    }

    var result = data.remove_attachments
      .Select(value => get_lead_attachments(id, value.Uuid).FirstOrDefault())
      .Where(attachment => attachment != null)
      .Select(file => file.Id)
      .ToList()
      .Select(delete_lead_attachment)
      .Where(x => x)
      .ToList();
    data.lead.Address = data.lead.Address.Trim();
    data.lead.Address = data.lead.Address.nl2br();
    data.lead.Email = data.lead.Email.Trim();
    db.Leads.Add(data.lead);
    var affected_rows = db.Leads.Where(x => x.Id == id)
      .Update(x => data);
    if (affected_rows <= 0) return affectedRows > 0;
    affectedRows++;
    if (current_status_id != data.lead.Status!.Id)
    {
      db.Leads.Where(x => x.Id == id)
        .Update(x => new Lead
        {
          LastStatusChange = today()
        });
      var row = get_status(x => x.Id == data.lead.Status!.Id).FirstOrDefault() ?? new LeadsStatus();
      var new_status_name = row.Name;
      log_lead_activity(id, "not_lead_activity_status_updated", false, JsonConvert.SerializeObject(new
      {
        full_name = db.get_staff_full_name(),
        current_status,
        new_status_name
      }));
      hooks.do_action("lead_status_changed", new
      {
        lead_id = id,
        old_status = current_status_id,
        new_status = data.lead.Status
      });
    }

    if ((current_lead_data.lead.Junk || current_lead_data.lead.Lost) && data.lead.Status!.Id != 0)
      db.Leads.Where(x => x.Id == id)
        .Update(x => new Lead
        {
          Junk = false,
          Lost = false
        });
    if (data.lead.Assigned > 0)
      if (current_lead_data.lead.Assigned != data.lead.Assigned && data.lead.Assigned != 0 && data.lead.Assigned != 0)
        lead_assigned_member_notification(id, data.lead.Assigned);
    log_activity($"Lead Updated [ID: {id}]");
    return true;
  }

  public bool delete(int id)
  {
    var affectedRows = 0;
    hooks.do_action("before_lead_deleted", id);
    var result = get(x => x.Id == id).First();
    var affected_rows = db.Leads.Where(x => x.Id == id).Delete();
    if (affected_rows > 0)
    {
      log_activity($"Lead Deleted [Deleted by: {db.get_staff_full_name()}, ID: {id}]");
      var attachments = get_lead_attachments(id);
      attachments.ToList().ForEach(x => { delete_lead_attachment(x.Id); });
      db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "leads").Delete();
      db.LeadActivityLogs.Where(x => x.LeadId == id).Delete();
      db.LeadIntegrationEmails.Where(x => x.Id == id).Delete();
      db.Notes.Where(x => x.RelId == id && x.RelType == "lead").Delete();
      db.Reminders.Where(x => x.RelId == id && x.RelType == "lead").Delete();
      db.Taggables.Where(x => x.RelId == id && x.RelType == "lead").Delete();
      db.Proposals
        .Where(x => x.RelId == id && x.RelType == "lead")
        .ToList()
        .ForEach(x => { proposals_model.delete(x.Id); });
      var tasks = db.Tasks.Where(x => x.RelId == id && x.RelType == "lead").ToList();
      tasks.ForEach(task => { tasks_model.delete_task(task.Id); });
      if (self.helper.is_gdpr())
        db.ActivityLogs
          .Where(x =>
            x.Description.Contains(result.lead.Email) ||
            x.Description.Contains(result.lead.Name) ||
            x.Description.Contains(result.lead.PhoneNumber)
          )
          .Delete();
      affectedRows++;
    }

    if (affectedRows <= 0) return false;
    hooks.do_action("after_lead_deleted", id);
    return true;
  }

  public bool mark_as_lost(int id)
  {
    var last_lead_status = db.Leads.FirstOrDefault(x => x.Id == id)!.Status;
    var affected_rows = db.Leads
      .Where(x => x.Id == id)
      .Update(x => new Lead
      {
        Lost = true,
        StatusId = 0,
        LastStatusChange = today(),
        LastLeadStatus = last_lead_status.Id
      });
    if (affected_rows <= 0) return false;
    log_lead_activity(id, "not_lead_activity_marked_lost");
    log_activity($"Lead Marked as Lost [ID: {id}]");
    hooks.do_action("lead_marked_as_lost", id);
    return true;
  }

  public bool unmark_as_lost(int id)
  {
    // Retrieve the last lead status
    var lastLeadStatus = db.Leads
      .Where(lead => lead.Id == id)
      // .Select(lead => lead.LastLeadStatus)
      .FirstOrDefault();
    if (lastLeadStatus == null) return false; // or handle the case where the lead does not exist
    // Update the lead's lost status and restore the last status
    var leadToUpdate = db.Leads.Find(id);
    if (leadToUpdate == null) return false;
    leadToUpdate.Lost = false; // Assuming Lost is a boolean property
    leadToUpdate.Status = lastLeadStatus.Status;
    db.SaveChanges(); // Persist changes
    log_lead_activity(id, "not_lead_activity_unmarked_lost");
    log_activity($"Lead Unmarked as Lost [ID: {id}]");
    return true;
  }

  public async Task<bool> mark_as_junk(int id)
  {
    var row = db.Leads.FirstOrDefault(x => x.Id == id);
    var last_lead_status = row.StatusId.Value;
    var affected_rows = await db.Leads
      .Where(x => x.Id == id)
      .UpdateAsync(x => new Lead
      {
        Junk = true,
        StatusId = 0,
        LastStatusChange = today(),
        LastLeadStatus = last_lead_status
      });
    if (affected_rows <= 0) return false;
    log_lead_activity(id, "not_lead_activity_marked_junk");
    log_activity($"Lead Marked as Junk [ID: {id}]");
    hooks.do_action("lead_marked_as_junk", id);
    return true;
  }

  public bool unmark_as_junk(int id)
  {
    var row = db.Leads.FirstOrDefault(x => x.Id == id);
    var last_lead_status = row.LastLeadStatus;
    var affected_rows = db.Leads.Where(x => x.Id == id)
      .Update(x => new Lead
      {
        Junk = false,
        StatusId = last_lead_status
      });
    if (affected_rows <= 0) return false;
    log_lead_activity(id, "not_lead_activity_unmarked_junk");
    log_activity($"Lead Unmarked as Junk [ID: {id}]");
    return true;
  }

  public List<File> get_lead_attachments(int id = 0, string attachment_id = "", Expression<Func<File, bool>> where = default)
  {
    var query = db.Files.Where(where).AsQueryable();
    var idIsHash = !string.IsNullOrEmpty(attachment_id) && attachment_id.Length == 32;
    //
    //     query.Where(x=>x.AttachmentKey==attachment_id);
    //     query.Where(x=>x.Id==id);
    //   return query.FirstOrDefault();
    // db.where("rel_type", "lead");
    var rows = query
      .Where(x => x.RelId == id && x.RelType == "lead")
      .OrderByDescending(x => x.DateCreated)
      .ToList();
    return rows;
  }

  public void add_attachment_to_database(int lead_id, File attachment, string? external = null, bool form_activity = false)
  {
    misc_model.add_attachment_to_database(lead_id, "lead", attachment, external);
    if (form_activity == false)
      log_lead_activity(lead_id, "not_lead_activity_added_attachment");
    else
      log_lead_activity(lead_id, "not_lead_activity_log_attachment", true, JsonConvert.SerializeObject(new[]
      {
        form_activity
      }));
    if (form_activity) return;
    var result = get(x => x.Id == lead_id).First();
    var not_user_ids = new List<int>();
    if (result.lead.AddedFrom != staff_user_id)
      not_user_ids.Add(result.lead.AddedFrom);
    if (result.lead.Assigned != staff_user_id && result.lead.Assigned != 0)
      not_user_ids.Add(result.lead.Assigned);
    var notifiedUsers = not_user_ids
      .Select(x =>
      {
        var notified = db.add_notification(new Notification
        {
          Description = "not_lead_added_attachment",
          ToUserId = x,
          Link = $"#leadid={lead_id}",
          AdditionalData = JsonConvert.SerializeObject(new[]
          {
            result.lead.Name
          })
        });
        return notified ? x : 0;
      })
      .ToList()
      .Where(x => x > 0)
      .ToList();
    db.pusher_trigger_notification(notifiedUsers);
  }

  public bool delete_lead_attachment(int id)
  {
    var attachment = get_lead_attachments(id).FirstOrDefault();
    var deleted = false;
    if (attachment == null) return deleted;
    if (string.IsNullOrEmpty(attachment.External))
      unlink($"{get_upload_path_by_type("lead")}{attachment.RelId}/{attachment.FileName}");
    var affected_rows = db.Files.Where(x => x.Id == attachment.Id).Delete();
    if (affected_rows > 0)
    {
      deleted = true;
      log_activity($"Lead Attachment Deleted [ID: {attachment.RelId}]");
    }

    if (!is_dir(get_upload_path_by_type("lead") + attachment.RelId)) return deleted;
    var other_attachments = list_files(get_upload_path_by_type("lead") + attachment.RelId);
    if (!other_attachments.Any())
      delete_dir(get_upload_path_by_type("lead") + attachment.RelId);
    return deleted;
  }

  public List<LeadsSource> get_source()
  {
    var rows = db.LeadsSources
      .OrderBy(x => x.Name)
      .ToList();
    return rows;
  }

  public LeadsSource? get_source(int id)
  {
    var row = db.LeadsSources
      .FirstOrDefault(x => x.Id == id);
    return row;
  }

  public int add_source(LeadsSource data)
  {
    var result = db.LeadsSources.Add(data);
    if (result.IsAdded())
      log_activity($"New Leads Source Added [SourceID: {result.Entity.Id}, Name: {data.Name}]");
    return result.Entity.Id;
  }

  public bool update_source(LeadsSource data)
  {
    var id = data.Id;
    var affected_rows = db.LeadsSources
      .Where(x => x.Id == id)
      .Update(x => data);
    if (affected_rows <= 0) return false;
    log_activity($"Leads Source Updated [SourceID: {id}, Name: {data.Name}]");
    return true;
  }

  public bool delete_source(int id)
  {
    var current = get_source(id);
    var affected_rows = db.LeadsSources
      .Where(x => x.Id == id)
      .Delete();
    if (affected_rows <= 0) return false;
    if (db.get_option_compare("leads_default_source", id))
      db.update_option("leads_default_source", "");
    log_activity($"Leads Source Deleted [SourceID: {id}]");
    return true;
  }

  public List<LeadsStatus> get_status(Expression<Func<LeadsStatus, bool>> condition)
  {
    var query = db.LeadsStatuses
      .Where(condition)
      .AsQueryable();
    //   db.where("id", id);
    // }
    var statuses = app_object_cache.get<List<LeadsStatus>>("leads-all-statuses");
    if (statuses == null)
      query.OrderBy(x => x.StatusOrder);
    statuses = query.ToList();
    app_object_cache.add("leads-all-statuses", statuses);
    return statuses;
  }

  public int add_status(LeadsStatus data)
  {
    if (string.IsNullOrEmpty(data.Color))
      data.Color = hooks.apply_filters("default_lead_status_color", "#757575");
    data.StatusOrder ??= db.LeadsStatuses.Count() + 1;
    var result = db.LeadsStatuses.Add(data);
    if (!result.IsAdded()) return 0;
    log_activity($"New Leads Status Added [StatusID: {result.Entity.Id}, Name: {data.Name}]");
    return result.Entity.Id;
  }

  public bool update_status(LeadsStatus data)
  {
    var id = data.Id;
    var affected_rows = db.LeadsStatuses.Where(x => x.Id == id)
      .Update(x => data);
    if (affected_rows <= 0) return false;
    log_activity($"Leads Status Updated [StatusID: {id}, Name: {data.Name}]");
    return true;
  }

  public bool delete_status(int id)
  {
    var current = get_status(x => x.Id == id);
    // if (is_reference_in_table("status",  "leads", id) || is_reference_in_table("lead_status",  "leads_email_integration", id))
    //     "referenced" => true
    var affected_rows = db.LeadsStatuses.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    if (db.get_option_compare("leads_default_status", id))
      db.update_option("leads_default_status", "");
    log_activity($"Leads Status Deleted [StatusID: {id}]");
    return true;
  }

  public bool update_lead_status(Lead data)
  {
    var _old = db.Leads.FirstOrDefault(x => x.Id == data.Id);
    if (_old == null) return false;
    var old_status = new LeadsStatus();
    var old_status_name = string.Empty;
    if (_old != null)
    {
      old_status = get_status(x => x.Id == _old.StatusId).FirstOrDefault();
      old_status_name = old_status?.Name;
    }

    var affectedRows = 0;
    var current_status = get_status(x => x.Id == data.Id).First().Name;
    var affected_rows = db.Leads
      .Where(x => x.Id == data.Id)
      .Update(x => new Lead
      {
        StatusId = data.StatusId
      });
    var _log_message = "";
    if (affected_rows > 0)
    {
      affectedRows++;
      if (current_status != old_status_name && old_status_name != "")
      {
        _log_message = "not_lead_activity_status_updated";
        var additional_data = JsonConvert.SerializeObject(new[]
        {
          db.get_staff_full_name(),
          old_status_name,
          current_status
        });
        hooks.do_action("lead_status_changed", new
        {
          lead_id = data.Id,
          old_status_name,
          new_status = current_status
        });
        log_lead_activity(data.Id, _log_message, false, additional_data);
      }

      db.Leads.Where(x => x.Id == data.Id)
        .Update(x => new Lead
        {
          LastStatusChange = today()
        });
    }

    if (affectedRows <= 0) return false;
    if (_log_message == "") return true;
    log_lead_activity(data.Id, _log_message, false, "");
    return true;
  }

  /**
   * All lead activity by staff
   * @param  mixed id lead id
   * @return array
   */
  public List<LeadActivityLog> get_lead_activity_log(int id)
  {
    var sorting = hooks.apply_filters("lead_activity_log_default_sort", "ASC");
    var query = db.LeadActivityLogs
      .Where(x => x.LeadId == id).AsQueryable();
    if (sorting == "ASC")
      query.OrderBy(x => x.Date);
    else
      query.OrderByDescending(x => x.Date);
    var rows = query.ToList();
    return rows;
  }

  public bool staff_can_access_lead(int id, int? staff_id = null)
  {
    staff_id = staff_id.HasValue ? staff_user_id : staff_id;
    var view_lead = db.has_permission("leads", staff_id, "view");
    if (view_lead) return true;
    var output = db.Leads
      .Any(x => x.Id == id &&
                (x.Assigned == staff_id || x.IsPublic || x.AddedFrom == staff_id)
      );
    return output;
  }

  public int log_lead_activity(int id, string description, bool integration = false, string additional_data = "")
  {
    var log = new LeadActivityLog
    {
      Date = DateTime.Now,
      Description = description,
      LeadId = id,
      StaffId = staff_user_id,
      AdditionalData = additional_data,
      FullName = db.get_staff_full_name(staff_user_id)
    };
    if (integration)
    {
      log.StaffId = 0;
      log.FullName = "[CRON]";
    }

    var result = db.LeadActivityLogs.Add(log);
    return result.Entity.Id;
  }

  public LeadsEmailIntegration? get_email_integration()
  {
    var row = db.LeadsEmailIntegrations.FirstOrDefault(x => x.Id == 1);
    return row;
  }

  public List<LeadIntegrationEmail> get_mail_activity(int id)
  {
    var rows = db.LeadIntegrationEmails
      .Where(x => x.Id == id)
      .OrderBy(x => x.DateCreated)
      .ToList();
    return rows;
  }

  public bool update_email_integration(LeadsEmailIntegration data, LeadOption option)
  {
    // Assuming db is your database context
    var originalSettings = db.LeadsEmailIntegrations.FirstOrDefault(e => e.Id == 1);
    // Setting the properties based on the incoming data
    // data.CreateTaskIfCustomer = data.CreateTaskIfCustomer ? 1 : 0;
    // data.Active = data.Active ? 1 : 0;
    // data.DeleteAfterImport = data.DeleteAfterImport ? 1 : 0;
    // data.NotifyLeadImported = data.NotifyLeadImported ? 1 : 0;
    // data.OnlyLoopOnUnseenEmails = data.OnlyLoopOnUnseenEmails ? 1 : 0;
    // data.NotifyLeadContactMoreTimes = data.NotifyLeadContactMoreTimes ? 1 : 0;
    // data.MarkPublic = data.MarkPublic ? 1 : 0;
    // data.Responsible = data.Responsible ?? 0;
    if (data.NotifyLeadContactMoreTimes != 0 || data.NotifyLeadImported != 0)
    {
      if (data.NotifyType == "specific_staff")
        data.NotifyIds = option.NotifyIdsStaff != null
          ? JsonConvert.SerializeObject(option.NotifyIdsStaff)
          : JsonConvert.SerializeObject(new List<int>());
      else
        data.NotifyIds = option.NotifyIdsRoles != null
          ? JsonConvert.SerializeObject(option.NotifyIdsRoles)
          : JsonConvert.SerializeObject(new List<string>());
    }
    else
    {
      data.NotifyIds = JsonConvert.SerializeObject(new List<int>());
      data.NotifyType = null;
    }

    // Handle password logic
    if (!string.IsNullOrEmpty(data.Password))
    {
      var originalDecryptedPassword = self.helper.decrypt(originalSettings.Password, self.config.get<string>("aes_key", ""));
      data.Password = originalDecryptedPassword == data.Password
        ? null
        : self.helper.encrypt(data.Password, self.config.get<string>("aes_key", ""));
    }

    // Update the database
    var result = db.LeadsEmailIntegrations
      .Where(e => e.Id == 1)
      .Update(x => data);
    return result > 0;
  }

  public void change_status_color(LeadsStatus data)
  {
    db.LeadsStatuses
      .Where(x =>
        x.Id == data.Id
      )
      .Update(x =>
        new LeadsStatus { Color = data.Color }
      );
  }

  public void update_status_order(params LeadsStatus[] data)
  {
    data
      .ToList()
      .ForEach(x =>
      {
        db.LeadsStatuses
          .Where(x => x.Id == x.Id)
          .Update(x => new LeadsStatus { StatusOrder = x.StatusOrder });
      });
  }

  public WebToLead? get_form(Expression<Func<WebToLead, bool>> condition)
  {
    var row = db.WebToLeads.Where(condition).FirstOrDefault();
    return row;
  }

  public int? add_form((WebToLead webToLead, LeadOption option) data)
  {
    data = _do_lead_web_to_form_responsibles(data);
    data.webToLead.SuccessSubmitMsg = data.webToLead.SuccessSubmitMsg!.nl2br();
    data.webToLead.FormKey = self.helper.uuid();
    // Convert boolean values to integers
    // data.CreateTaskOnDuplicate = data.CreateTaskOnDuplicate ? 1 : 0;
    // data.MarkPublic = data.MarkPublic ? 1 : 0;
    // Handle duplication logic
    if (data.webToLead.AllowDuplicate > 0)
    {
      data.webToLead.AllowDuplicate = 1;
      data.webToLead.TrackDuplicateField = string.Empty;
      data.webToLead.TrackDuplicateFieldAnd = string.Empty;
      data.webToLead.CreateTaskOnDuplicate = 0;
    }
    else
    {
      data.webToLead.AllowDuplicate = 0;
    }

    data.webToLead.DateCreated = DateTime.Now;
    // Assuming `db` is your database context
    var result = db.WebToLeads.Add(data.webToLead);
    var insertId = result.Entity.Id; // Replace this with the appropriate method to get the last inserted ID
    if (!result.IsAdded()) return null; // Return null if insertion fails
    log_activity($"New Web to Lead Form Added [{data.webToLead.Name}]");
    return insertId;
  }

  public bool update_form(int id, (WebToLead webToLead, LeadOption option) data)
  {
    // Process the data
    data = _do_lead_web_to_form_responsibles(data);
    data.webToLead.SuccessSubmitMsg = data.webToLead.SuccessSubmitMsg.nl2br();
    data.webToLead.CreateTaskOnDuplicate = Convert.ToInt32(data.webToLead.CreateTaskOnDuplicate);
    // data.webToLead.MarkPublic = data.webToLead.MarkPublic;
    if (Convert.ToBoolean(data.webToLead.AllowDuplicate))
    {
      data.webToLead.AllowDuplicate = 1;
      data.webToLead.TrackDuplicateField = "";
      data.webToLead.TrackDuplicateFieldAnd = "";
      data.webToLead.CreateTaskOnDuplicate = 0;
    }
    else
    {
      data.webToLead.AllowDuplicate = 0;
    }

    // Update the database
    var affectedRows = db.WebToLeads.Where(x => x.Id == id).Update(x => data);
    return affectedRows > 0;
  }

  public bool delete_form(int id)
  {
    db.WebToLeads
      .Where(x => x.Id == id)
      .Delete();
    var affected_rows = db.Leads
      .Where(x => x.FromFormId == id)
      .Update(x => new Lead { FromFormId = 0 });
    if (affected_rows <= 0) return false;
    log_activity($"Lead Form Deleted [{id}]");
    return true;
  }

  // private WebToLead _do_lead_web_to_form_responsibles(WebToLead data)
  // {
  //     data.NotifyLeadImported = data.NotifyLeadImported>0 ? 1 : 0;
  //
  //     if (data.Responsible <0)
  //       data.Responsible = 0;
  //     if (data.NotifyLeadImported != 0) {
  //         if (data.NotifyType == "specific_staff") {
  //             if ( data["notify_ids_staff"]) {
  //                 data.NotifyIds = JsonConvert.SerializeObject(data["notify_ids_staff"]);
  //                  (data["notify_ids_staff"]);
  //             } else {
  //                 data.NotifyIds = JsonConvert.SerializeObject(new List<int>());
  //                  (data["notify_ids_staff"]);
  //             }
  //             if ( data["notify_ids_roles"])  (data["notify_ids_roles"]);
  //         } else {
  //             if ( data["notify_ids_roles"]) {
  //                 data.NotifyIds = JsonConvert.SerializeObject(data["notify_ids_roles"]);
  //                  (data["notify_ids_roles"]);
  //             } else {
  //                 data.NotifyIds = JsonConvert.SerializeObject(new List<string>());
  //                  (data["notify_ids_roles"]);
  //             }
  //             if ( data["notify_ids_staff"])  (data["notify_ids_staff"]);
  //         }
  //     } else {
  //         data.NotifyIds  = JsonConvert.SerializeObject(new List<int>(){});
  //         data.NotifyType = null;
  //         if ( data["notify_ids_staff"])  (data["notify_ids_staff"]);
  //         if ( data["notify_ids_roles"])  (data["notify_ids_roles"]);
  //     }
  //
  //     return data;
  // }
  private (WebToLead webToLead, LeadOption option) _do_lead_web_to_form_responsibles((WebToLead webToLead, LeadOption option) data)
  {
    // Ensure NotifyLeadImported is either 0 or 1
    data.webToLead.NotifyLeadImported = data.webToLead.NotifyLeadImported > 0 ? 1 : 0;
    // Ensure Responsible is non-negative
    data.webToLead.Responsible = Math.Max(data.webToLead.Responsible, 0);
    // Process notification settings based on NotifyLeadImported
    if (data.webToLead.NotifyLeadImported != 0)
    {
      if (data.webToLead.NotifyType == "specific_staff")
      {
        // Handle specific staff notifications
        data.webToLead.NotifyIds = data.option.NotifyIdsStaff.Any()
          ? string.Join(",", data.option.NotifyIdsStaff)
          : string.Empty;
        // Process notify_ids_roles if available
        if (data.option.NotifyIdsRoles.Any())
        {
          // Handle roles if needed
        }
      }
      else
      {
        // Handle role notifications
        data.webToLead.NotifyIds = data.option.NotifyIdsRoles is { Count: > 0 }
          ? string.Join(",", data.option.NotifyIdsRoles)
          : string.Empty;
        // Process notify_ids_staff if available
        if (data.option.NotifyIdsStaff.Any())
        {
          // Handle staff if needed
        }
      }
    }
    else
    {
      // Reset NotifyIds and NotifyType if no notifications
      data.webToLead.NotifyIds = string.Empty;
      data.webToLead.NotifyType = string.Empty;
      // Process notify_ids_staff and notify_ids_roles
      if (data.webToLead.NotifyIds.Any())
      {
        // Handle staff if needed
      }

      if (data.option.NotifyIdsRoles.Any())
      {
        // Handle roles if needed
      }
    }

    return data;
  }

  public void do_kanban_query(int status, string search = "", int page = 1, object? sort = null, bool count = false)
  {
    //
    //                .search(search)
    //      .sortBy(sort["sort"] ?? null, sort["sort_by"] ?? null);
    // if (count) return kanBan.countAll();
    // return kanBan.get();
  }

  public async void lead_assigned_member_notification(int lead_id, int assigned = 0, bool integration = false)
  {
    if (assigned == 0) return;
    if (integration == false)
      if (assigned == db.get_staff_user_id())
        return;
    var name = db.Leads.FirstOrDefault(x => x.Id == lead_id)?.Name;
    var notification_data = new Notification
    {
      Description = integration == false ? "not_assigned_lead_to_you" : "not_lead_assigned_from_form",
      ToUserId = assigned,
      Link = $"#leadid={lead_id}",
      AdditionalData = integration == false
        ? JsonConvert.SerializeObject(new[] { name })
        : JsonConvert.SerializeObject(new List<string>())
    };
    if (integration) notification_data.FromCompany = true;
    if (db.add_notification(notification_data))
      db.pusher_trigger_notification(new List<int> { assigned });
    var row = db.Staff.First(x => x.Id == assigned);
    var email = row.Email;
    self.helper.send_mail_template("lead_assigned", lead_id, email);
    await db.Leads
      .Where(x => x.Id == lead_id)
      .UpdateAsync(x => new Lead
      {
        DateAssigned = DateTime.Now
      });
    await db.SaveChangesAsync();
    var not_additional_data = new List<string>
    {
      db.get_staff_full_name(),
      "<a href='" + self.navigation.admin_url($"profile/{assigned}") + "' target='_blank'>" + db.get_staff_full_name(assigned) + "</a>"
    };
    // if (integration)
    // {
    //   unset(not_additional_data[0]);
    //   array_values(not_additional_data);
    // }
    // not_additional_data = JsonConvert.SerializeObject(not_additional_data);
    var not_desc = integration == false ? "not_lead_activity_assigned_to" : "not_lead_activity_assigned_from_form";
    log_lead_activity(lead_id, not_desc, integration, JsonConvert.SerializeObject(not_additional_data));
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  public (MyInstance, MyContext ) getInstance()
  {
    return (self, db);
  }
}
