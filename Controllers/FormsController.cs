using System.Dynamic;
using System.Linq.Expressions;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Core.InputSet;
using Service.Framework.Library.Merger;
using Service.Helpers;
using Service.Helpers.Countries;
using Service.Helpers.Database;
using Service.Helpers.Recaptcha;
using Service.Libraries.Documents;
using Service.Models.Tickets;

namespace Service.Controllers;

[ApiController]
[Route("api/forms")]
public class FormsController(ILogger<EstimateController> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  [HttpGet]
  public IActionResult Index()
  {
    return NotFound();
  }

  /**
 * Web to lead form
 * User no need to see anything like LEAD in the url, this is the reason the method is named wtl
 * @param  string key web to lead form key identifier
 * @return mixed
 */
  [HttpGet("wtl/{name}")]
  public async Task<IActionResult> Wtl(string name)
  {
    var leads_model = self.leads_model(db);
    var form = leads_model.get_form(x => x.FormKey == name);

    if (form == null) return NotFound();

    var data = new ExpandoObject() as dynamic;
    data.FormFields = JsonConvert.DeserializeObject<List<string>>(form.FormData);
    if (data.FormFields == null) data.FormFields = new List<string>();

    if (self.input.post_has("key") && self.input.post("key") == name)
    {
      var postData = self.input.post<Dictionary<string, object>>();
      var required = new List<string>();

      foreach (var field in data.FormFields)
        if (field.Contains("required"))
          required.Add(field);

      if (db.is_gdpr() && db.get_option<int>("gdpr_enable_terms_and_conditions_lead_form") == 1)
        required.Add("accept_terms_and_conditions");

      if (required.Any(field => !postData.ContainsKey(field) || string.IsNullOrEmpty(Convert.ToString(postData[field]))))
      {
        self.output.set_status_header(HttpStatusCode.UnprocessableEntity);
        return MakeError("This page is currently disabled, check back later.");
      }

      if (!string.IsNullOrEmpty(db.get_option("recaptcha_secret_key")) && !string.IsNullOrEmpty(db.get_option("recaptcha_site_key")) && form.Recaptcha == 1)
        if (!self.helper.do_recaptcha_validation(Convert.ToString(postData["gecaptcha-response"])))
          return MakeSuccess(new
          {
            success = false,
            message = label("recaptcha_error")
          });

      var regularFields = new Lead();
      var customFields = new List<CustomField>();

      foreach (var key in postData.Keys)
      {
        var val = postData[key];
        if (key.Contains("form-cf-"))
        {
          customFields.Add(new CustomField() { Name = key, DefaultValue = $"{val}" });
        }
        else
        {
          if (name == "country")
          {
            if (self.helper.is_numeric(val)) continue;
            if (val.ToString() == "")
            {
              //   regularFields[name] = 0;
            }
            else
            {
              var country = db.Countries.FirstOrDefault(x => x.Iso2 == val.ToString() || x.ShortName == val.ToString() || x.LongName == val.ToString());
              val = country != null ? country.Id : 0;
            }
          }
          else if (name == "address")
          {
            val = val.ToString().Trim();
            val = val.ToString().Replace("\n", "<br />");
          }

          // regularFields[name] = val;
        }
      }

      int? taskId = null;
      var success = false;
      var insertToDb = true;

      if (form.AllowDuplicate == 0)
      {
        // var condition = new Expression<Func<Lead, bool>>(x => x.Id == 0);
        Expression<Func<Lead, bool>> condition = x => x.Id == 0;
        if (condition != null)
        {
          var total = db.Leads.Count(condition);
          Lead duplicateLead = null;
          if (total == 1) duplicateLead = db.Leads.First(condition);
          if (total > 0)
          {
            success = true;
            insertToDb = false;
            if (form.CreateTaskOnDuplicate == 1)
            {
              var taskName = regularFields.Name ?? regularFields.Email ?? regularFields.Company ?? form.Name;
              var description = form.Name + "<br /><br />";
              var customFieldsParsed = customFields.ToDictionary(field => field.Name, field => field.DefaultValue ?? string.Empty);
              var allFields = (Dictionary<string, object>)TypeMerger.Merge(regularFields, customFieldsParsed);

              foreach (var kvp in allFields)
              {
                var _name = kvp.Key;
                var val = kvp.Value;
                if (!data.FormFields.Contains(_name)) continue;
                if (name == "country" && self.helper.is_numeric(val))
                {
                  var country = db.get_country((int)val);
                  val = country != null ? country.ShortName : "Unknown";
                }

                description += $"{_name}: {val}<br />";
              }

              var taskData = new Entities.Task()
              {
                Name = taskName,
                Priority = db.get_option<int>("default_task_priority"),
                DateCreated = DateTime.Now,
                StartDate = DateTime.Now,
                AddedFrom = form.Responsible,
                Status = 1,
                Description = description
              };

              taskData = hooks.apply_filters("before_add_task", taskData);
              var result = await db.Tasks.AddAsync(taskData);
              await db.SaveChangesAsync();
              taskId = result.State == EntityState.Added ? taskData.Id : (int?)null;

              if (taskId.HasValue)
              {
                var tasks_model = self.tasks_model(db);
                var attachments = self.helper.handle_task_attachments_array(taskId.Value, "file-input");
                attachments.ForEach(x => tasks_model.add_attachment_to_database(taskId.Value, x, null, false));
                tasks_model.add_task_assignees(db.task_assigned(new TaskAssigned { TaskId = taskId.Value, AssignedFrom = form.Responsible }), true);
                hooks.do_action("after_add_task", taskId);

                if (string.IsNullOrEmpty(duplicateLead.Email)) db.send_mail_template("lead_web_form_submitted", duplicateLead);
              }
            }
          }
        }
      }

      var leadId = 0;
      if (insertToDb)
      {
        regularFields.StatusId = form.LeadStatus;
        regularFields.Name = string.IsNullOrEmpty(regularFields.Name) ? "Unknown" : regularFields.Name;
        regularFields.SourceId = form.LeadSource;
        regularFields.AddedFrom = 0;
        regularFields.LastContact = null;
        regularFields.Assigned = form.Responsible;
        regularFields.DateCreated = DateTime.Now;
        regularFields.FromFormId = form.Id;
        regularFields.IsPublic = form.MarkPublic;
        db.Leads.Add(regularFields);
        await db.SaveChangesAsync();
        leadId = regularFields.Id;
        hooks.do_action("lead_created", new { leadId, web_to_lead_form = true });

        success = leadId > 0;
        if (success)
        {
          leads_model.log_lead_activity(leadId, "not_lead_imported_from_form", true, JsonConvert.SerializeObject(new[] { form.Name }));

          var customFieldsBuild = new Dictionary<string, Dictionary<string, string>>
          {
            ["leads"] = customFields.ToDictionary(cf => cf.Name.Replace("form-cf-", ""), cf => cf.DefaultValue)
          };

          leads_model.lead_assigned_member_notification(leadId, form.Responsible, true);
          self.helper.handle_custom_fields_post(leadId, customFieldsBuild);
          self.helper.handle_lead_attachments(leadId, "file-input", string.IsNullOrEmpty(form.Name));

          if (form.NotifyLeadImported != 0)
          {
            var field = form.NotifyType switch
            {
              "specific_staff" => "staffid",
              "roles" => "role",
              _ => null
            };

            var staff = form.NotifyType == "assigned"
              ? new List<Staff> { new() { Id = form.Responsible } }
              : db.Staff.Where(x => x.Active.HasValue && x.Active.Value && form.NotifyIds != null && JsonConvert.DeserializeObject<List<int>>(form.NotifyIds).Contains(x.Id)).ToList();

            var notifiedUsers = staff.Select(member =>
            {
              if (member.Id == 0) return 0;
              var notified = db.add_notification(new Notification()
              {
                Description = "not_lead_imported_from_form",
                ToUserId = member.Id,
                FromCompany = true,
                FromUserId = 0,
                AdditionalData = JsonConvert.SerializeObject(new[] { form.Name }),
                Link = "#leadid=" + leadId
              });
              return notified ? member.Id : 0;
            }).ToList();

            db.pusher_trigger_notification(notifiedUsers);
          }

          if (!string.IsNullOrEmpty(regularFields.Email))
          {
            var lead = leads_model.get(x => x.Id == leadId);
            db.send_mail_template("lead_web_form_submitted", lead);
          }
        }
      }

      hooks.do_action("web_to_lead_form_submitted", new { leadId, formId = form.Id, taskId });

      return MakeSuccess(new
      {
        success,
        message = form.SuccessSubmitMsg
      });
    }

    data.Form = form;
    return MakeSuccess(data);
  }

  /**
 * Web to lead form
 * User no need to see anything like LEAD in the url, this is the reason the method is named eq lead
 * @param  string hash lead unique identifier
 * @return mixed
 */
  [HttpGet("l/{hash}")]
  public IActionResult L(string hash)
  {
    var gdpr_model = self.gdpr_model(db);
    var leads_model = self.leads_model(db);

    if (db.get_option("gdpr_enable_lead_public_form") == "0")
      return NotFound();

    var result = leads_model.get(x => x.Hash == hash).FirstOrDefault();

    if (result.lead == null)
      return NotFound();

    dynamic _result = self.helper.array_to_object(result.attachments[0]);
    if (self.input.post_has("update"))
    {
      var data = self.input.post<dynamic>();
      leads_model.update(data, result.lead.Id);
      return Redirect(globals("HTTP_REFERER"));
    }

    if (self.input.post_has("export") && db.get_option("gdpr_data_portability_leads") == "1")
    {
      // self.Load.Library("gdpr/gdpr_lead");
      // gdpr_lead.Export(result.Lead.Id);
    }
    else if (self.input.post_has("removal_request"))
    {
      var success = gdpr_model.add_removal_request(new GdprRequest()
      {
        Description = self.input.post<string>("removal_description").Replace("\n", "<br />"),
        RequestFrom = result.lead.Name,
        LeadId = result.lead.Id
      });

      if (!success)
        return Redirect(globals<string>("HTTP_REFERER"));

      // self.helper.SendGdprEmailTemplate("gdpr_removal_request_by_lead", result.Lead.Id);
      set_alert("success", label("data_removal_request_sent"));

      return Redirect(globals<string>("HTTP_REFERER"));
    }

    result.attachments = leads_model.get_lead_attachments(result.lead.Id);
    // self.DisableNavigation();
    // self.DisableSubMenu();
    // data.Title = result.Lead.Name;
    // data.Lead = result.Lead;
    // self.View("forms/lead");
    // data(data);
    // self.Layout(true);
    return MakeError();
  }

  [HttpGet("ticket")]
  public async Task<IActionResult> Ticket()
  {
    var appPath = string.Empty;
    var tickets_model = self.tickets_model(db);

    dynamic form = new ExpandoObject();
    form.Language = db.get_option("active_language");
    form.Recaptcha = 1;
    // self.Lang.Load("ticket_form_lang", form.Language);
    // self.Lang.Load(form.Language + "_lang", form.Language);
    // if (self.helper.FileExists($"{appPath}language/{form.Language}/custom_lang.json"))
    //     self.Lang.Load("custom_lang", form.Language);

    form.SuccessSubmitMsg = label("success_submit_msg");
    form = hooks.apply_filters("ticket_form_settings", form);

    if (self.input.is_ajax_request())
    {
      var postData = self.input.post<Dictionary<string, object>>();
      var required = new List<string> { "subject", "department", "email", "name", "message", "priority" };
      if (db.is_gdpr() && db.get_option_compare("gdpr_enable_terms_and_conditions_ticket_form", 1))
        required.Add("accept_terms_and_conditions");

      if (required.Any(field => !postData.ContainsKey(field) || string.IsNullOrEmpty(Convert.ToString(postData[field]))))
      {
        self.output.set_status_header(HttpStatusCode.UnprocessableEntity);
        return MakeError();
      }

      if (!string.IsNullOrEmpty(db.get_option("recaptcha_secret_key")) && !string.IsNullOrEmpty(db.get_option("recaptcha_site_key")) && form.Recaptcha == 1)
        if (!self.helper.do_recaptcha_validation(Convert.ToString(postData["gecaptcha-response"])))
          return MakeSuccess(new
          {
            success = false,
            message = label("recaptcha_error")
          });

      var reqData = convert<Ticket>(postData);

      var ticket = new Ticket()
      {
        Email = reqData.Email,
        Name = reqData.Name,
        Subject = reqData.Subject,
        Department = reqData.Department,
        Priority = reqData.Priority,
        Service = reqData.Service.HasValue && self.helper.is_numeric(reqData.Service) ? reqData.Service : null,
        Message = reqData.Message
      };

      var success = false;
      var result = db.Contacts.FirstOrDefault(x => x.Email == ticket.Email);
      if (result != null)
      {
        ticket.UserId = result.UserId;
        ticket.ContactId = result.Id;
      }

      postData = hooks.apply_filters("ticket_external_form_insert_data", postData);
      var ticketId = await tickets_model.add(convert<TicketOption>(postData));
      success = ticketId > 0;
      if (success)
        hooks.do_action("ticket_form_submitted", new { ticketId });

      return MakeSuccess(new
      {
        success,
        message = form.SuccessSubmitMsg
      });
    }

    var departments_model = self.departments_model(db);
    dynamic data = new ExpandoObject();
    data.Departments = departments_model.get();
    data.Priorities = tickets_model.get_priority();
    data.Priorities.CallbackTranslate = "ticket_priority_translate";
    data.Services = tickets_model.get_service();
    data.Form = form;
    // self.Load.View("forms/ticket", data);
    return MakeSuccess(data);
  }
}
