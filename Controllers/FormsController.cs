using System.Xml.Linq;
using Global.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Helpers;
using Service.Helpers.Tags;

namespace Service.Controllers;

public class FormsController(ILogger<FormsController> logger, MyInstance self) : ClientControllerBase(logger, self)

{
  public IActionResult Index()
  {
    return NotFound();
  }

  public IActionResult Wtl(string key)
  {
    var leads_model = self.model.leads_model();
    var form = leads_model.GetFormByKey(key);
    if (form == null) return NotFound();

    var formFields = JsonConvert.DeserializeObject<List<FormField>>(form.FormData) ?? new List<FormField>();

    if (Request.HttpMethod == "POST" && Request.Form["key"] == key)
    {
      var postData = Request.Form;
      var requiredFields = formFields.Where(f => f.Required).Select(f => f.Name).ToList();

      if (self.helper.is_gdpr() && GdprHelper.IsTermsAndConditionsEnabledForLeadForm()) requiredFields.Add("accept_terms_and_conditions");

      foreach (var field in requiredFields)
        if (string.IsNullOrEmpty(postData[field]))
        {
          Response.StatusCode = 422;
          return null;
        }

      if (RecaptchaHelper.IsRecaptchaEnabled(form.Recaptcha) && !RecaptchaHelper.ValidateRecaptcha(postData["g-recaptcha-response"])) return Json(new { success = false, message = "Recaptcha error" });

      postData.Remove("g-recaptcha-response");
      postData.Remove("key");

      var regularFields = new Dictionary<string, string>();
      var customFields = new List<CustomField>();

      foreach (var entry in postData)
        if (entry.Key.StartsWith("form-cf-"))
        {
          customFields.Add(new CustomField { Name = entry.Key, Value = entry.Value });
        }
        else if (DbHelper.FieldExists(entry.Key, "leads"))
        {
          if (entry.Key == "country" && !int.TryParse(entry.Value, out _))
            entry.Value = DbHelper.GetCountryIdByName(entry.Value).ToString();
          regularFields[entry.Key] = entry.Value;
        }

      var success = false;
      var insertToDb = true;

      if (form.AllowDuplicate == 0)
      {
        var duplicateWhereClause = leads_model.GetDuplicateWhereClause(form, regularFields);
        var totalDuplicates = DbHelper.GetTotalRows("leads", duplicateWhereClause);

        if (totalDuplicates > 0)
        {
          success = true;
          insertToDb = false;

          if (form.CreateTaskOnDuplicate == 1)
          {
            var tasks_model = self.model.tasks_model();

            var taskData = tasks_model.CreateTaskData(form, regularFields, customFields);
            var taskId = tasks_model.AddTask(taskData);

            if (taskId > 0)
            {
              tasks_model.HandleTaskAttachments(taskId);
              tasks_model.AssignTask(taskId, form.Responsible);
              tasks_model.NotifyLeadSubmission(form, regularFields, taskId);
            }
          }
        }
      }

      if (insertToDb)
      {
        var leadId = leads_model.add(form, regularFields, customFields);

        if (leadId > 0)
        {
          success = true;
          leads_model.NotifyLeadCreation(form, leadId);
        }
      }

      return Json(new { success, message = form.SuccessSubmitMsg });
    }

    data.FormFields = formFields;
    data.Form = form;
    return View("WebToLead");
  }


  [HttpGet("l/{hash}")]
  [HttpPost("l/{hash}")]
  public IActionResult lpost(string hash)
  {
    var leads_model = self.model.leads_model();
    if (!GdprHelper.IsLeadPublicFormEnabled()) return NotFound();
    var lead = leads_model.get(x => x.Hash == hash).FirstOrDefault();
    if (lead.formData == null) return NotFound();
    self.helper.load_lead_language(lead.formData.Id);
    if (Request.Form["update"] != null)
    {
      leads_model.UpdateLead(Request.Form, lead.Id);
      return Redirect(Request.UrlReferrer.ToString());
    }
    else if (Request.Form["export"] != null && GdprHelper.IsDataPortabilityEnabledForLeads())
    {
      _gdprService.ExportLeadData(lead.Id);
    }
    else if (Request.Form["removal_request"] != null)
    {
      var success = _gdprService.AddRemovalRequest(lead, Request.Form["removal_description"]);
      if (success)
      {
        GdprHelper.SendRemovalRequestEmail(lead.Id);
        TempData["Success"] = "Data removal request sent.";
      }

      return Redirect(Request.UrlReferrer.ToString());
    }


    data.Lead = lead;
    return View("Lead");
  }

  [HttpGet]
  public IActionResult GetTicket()
  {
    var (self, db) = getInstance();
    var tickets_model = self.model.tickets_model();

    var form = new Ticket
    {
      Language = OptionHelper.GetActiveLanguage(),
      Recaptcha = 1,
      SuccessSubmitMsg = "Success submit message"
    };


    data.Departments = tickets_model.GetDepartments();
    data.Priorities = tickets_model.GetPriorities();
    data.Services = tickets_model.GetServices();
    data.Form = form;
    return View("Ticket");
  }

  [HttpPost]
  public IActionResult CreateTicket()
  {
    var (self, db) = getInstance();
    var tickets_model = self.model.tickets_model();
    var departments_model = self.model.departments_model();
    var form = new TicketForm
    {
      Language = OptionHelper.GetActiveLanguage(),
      Recaptcha = 1,
      SuccessSubmitMsg = "Success submit message"
    };

    if (Request.HttpMethod == "POST" && Request.IsAjaxRequest())
    {
      var postData = Request.Form;
      var requiredFields = new List<string> { "subject", "department", "email", "name", "message", "priority" };

      if (GdprHelper.IsGdprEnabled() && GdprHelper.IsTermsAndConditionsEnabledForTicketForm()) requiredFields.Add("accept_terms_and_conditions");

      if (requiredFields.Any(field => string.IsNullOrEmpty(postData[field])))
      {
        Response.StatusCode = 422;
        return null;
      }

      if (RecaptchaHelper.IsRecaptchaEnabled(form.Recaptcha) && !RecaptchaHelper.ValidateRecaptcha(postData["g-recaptcha-response"])) return Json(new { success = false, message = "Recaptcha error" });

      var ticketData = tickets_model.CreateTicketData(postData);
      var ticketId = tickets_model.AddTicket(ticketData);

      if (ticketId > 0) tickets_model.NotifyTicketSubmission(ticketId);

      return Json(new { success = ticketId > 0, message = form.SuccessSubmitMsg });
    }


    data.Departments = departments_model.get();
    data.Priorities = tickets_model.get_priority();
    data.Services = tickets_model.get_service();
    data.Form = form;
    return View("Ticket");
  }
}
