using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Framework.Library.Email;
using Service.Framework.Library.MergeFields;
using Service.Models;

namespace Service.Controllers.Admin;

[ApiController]
[Route("api/admin/email")]
public class EmailController(ILogger<EmailController> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{
  public EmailsModel emails_model { get; set; }
  public AppMailer email { get; set; }

  public override void Init()
  {
    emails_model = self.emails_model(db);
  }

  /* List all email templates */
  public IActionResult index()
  {
    if (!db.has_permission("email_templates", "", "view"))
      access_denied("email_templates");
    var langCheckings = db.get_option<Dictionary<string, bool>>("email_templates_language_checks");
    if (langCheckings == null)
      langCheckings = new Dictionary<string, bool>();
    // else
    //   langCheckings = JsonConvert.DeserializeObject(langCheckings);

    var email_templates_english = db.EmailTemplates.Where(x => x.Language == "english").ToList();
    foreach (var avLanguage in my_app.get_available_languages())
      if (avLanguage != "english")
        foreach (var template in email_templates_english)
        {
          // Result is cached and stored in database
          // This page may perform 1000 queries per request
          if (langCheckings.ContainsKey(template.Slug + "-" + avLanguage)) continue;
          var notExists = db.EmailTemplates.Any(x => x.Slug == template.Slug && x.Language == avLanguage);
          langCheckings[template.Slug + "-" + avLanguage] = true;
          if (!notExists) continue;
          var dataset = new EmailTemplate
          {
            Slug = template.Slug,
            Type = template.Type,
            Language = avLanguage,
            Name = template.Name + " [" + avLanguage + "]",
            Subject = template.Subject,
            Message = "",
            FromName = template.FromName,
            PlainText = template.PlainText,
            Active = template.Active,
            Order = template.Order
          };
          db.EmailTemplates.Add(dataset);
          db.SaveChanges();
        }

    db.update_option("email_templates_language_checks", JsonConvert.SerializeObject(langCheckings));

    data.staff = emails_model.get(x => x.Type == "staff" && x.Language == "english");
    data.credit_notes = emails_model.get(x => x.Type == "credit_note" && x.Language == "english");
    data.tasks = emails_model.get(x => x.Type == "tasks" && x.Language == "english");
    data.client = emails_model.get(x => x.Type == "client" && x.Language == "english");
    data.tickets = emails_model.get(x => x.Type == "ticket" && x.Language == "english");
    data.invoice = emails_model.get(x => x.Type == "invoice" && x.Language == "english");
    data.estimate = emails_model.get(x => x.Type == "estimate" && x.Language == "english");
    data.contracts = emails_model.get(x => x.Type == "contract" && x.Language == "english");
    data.proposals = emails_model.get(x => x.Type == "proposals" && x.Language == "english");
    data.projects = emails_model.get(x => x.Type == "project" && x.Language == "english");
    data.leads = emails_model.get(x => x.Type == "leads" && x.Language == "english");
    data.gdpr = emails_model.get(x => x.Type == "gdpr" && x.Language == "english");
    data.subscriptions = emails_model.get(x => x.Type == "subscriptions" && x.Language == "english");
    data.title = label("email_templates");
    data.hasPermissionEdit = db.has_permission("email_templates", "", "edit");
    return MakeResult(data);
  }

  /* Edit email template */
  [HttpGet]
  public IActionResult email_template(int? id)
  {
    if (!db.has_permission("email_templates", "", "view"))
      return access_denied("email_templates");
    if (id.HasValue == false)
      return Redirect(admin_url("emails"));

    var app_merge_fields = self.library.other_merge_fields(AppGlobal.ServiceProvider);
    // English is not included here
    data.available_languages = my_app.get_available_languages();
    if (!$"{data.available_languages[0]}".Contains("english"))
    {
      var key = string.Empty;
      data.available_languages.Remove(key);
    }


    data.available_merge_fields = app_merge_fields.all();
    data.template = emails_model.get_email_template_by_id(id.Value);
    var title = data.template.name;
    data.title = title;
    return MakeResult(data);
  }

  [HttpPost("email-template")]
  public IActionResult email_template_post(int? id)
  {
    if (!db.has_permission("email_templates", "", "view"))
      return access_denied("email_templates");
    if (id.HasValue == false)
      return Redirect(admin_url("emails"));
    if (!db.has_permission("email_templates", "", "edit"))
      access_denied("email_templates");
    var dataset = self.input.post<EmailTemplate>();
    var tmp = self.input.post<EmailTemplate>();
    // foreach (data.message as key => contents) {
    //   data.message[key] = tmp["message"][key];
    // }
    // foreach (data.subject as key => contents) {
    //   data.subject[key] = tmp.Subject[key];
    // }

    dataset.FromName = tmp.FromName;
    dataset.Id = id.Value;
    var success = emails_model.update(dataset);
    if (success) set_alert("success", label("updated_successfully", label("email_template")));
    return Redirect(admin_url("emails/email_template/" + id));
  }

  public IActionResult enable_by_type(string type)
  {
    if (db.has_permission("email_templates", "", "edit"))
      emails_model.mark_as_by_type(type, true);

    return Redirect(admin_url("emails"));
  }

  public IActionResult disable_by_type(string type)
  {
    if (db.has_permission("email_templates", "", "edit"))
      emails_model.mark_as_by_type(type, false);
    return Redirect(admin_url("emails"));
  }

  public IActionResult enable(int id)
  {
    if (!db.has_permission("email_templates", "", "edit")) return Redirect(admin_url("emails"));
    var template = emails_model.get_email_template_by_id(id);
    emails_model.mark_as(template.Slug, true);

    return Redirect(admin_url("emails"));
  }

  public IActionResult disable(int id)
  {
    if (!db.has_permission("email_templates", "", "edit")) return Redirect(admin_url("emails"));
    var template = emails_model.get_email_template_by_id(id);
    emails_model.mark_as(template.Slug, false);
    return Redirect(admin_url("emails"));
  }

  /* Since version 1.0.1 - test your smtp settings */
  [HttpPost]
  public IActionResult sent_smtp_test_email()
  {
    email = new AppMailer();
    // Simulate fake template to be parsed
    var template = new EmailTemplate();
    template.Message = db.get_option("email_header") + "This is test SMTP email. <br />If you received this message that means that your SMTP settings is set correctly." + db.get_option("email_footer");
    template.FromName = db.get_option("companyname") != "" ? db.get_option("companyname") : "TEST";
    template.Subject = "SMTP Setup Testing";
    template = this.parse_email_template(template);
    hooks.do_action("before_send_test_smtp_email");
    // email.initialize();
    email.initialize("smtp.example.com", 587, "user@example.com", "password", true);
    if (db.get_option("mail_engine") == "phpmailer")
    {
      email.set_debug_output(err =>
      {
        if (!string.IsNullOrEmpty(globals<string>("debug")))
          globals("debug", "");
        globals("debug", globals<string>("debug") + err + "<br />");
        return err;
      });
      email.set_smtp_debug(3);
    }

    email.set_newline(config_item<string>("newline"));
    email.set_crlf(config_item<string>("crlf"));
    email.from(db.get_option("smtp_email"), template.FromName);
    email.to(self.input.post<string>("test_email"));
    var systemBCC = db.get_option("bcc_emails");
    if (systemBCC != "") email.bcc(systemBCC);

    email.subject(template.Subject);
    email.message(template.Message);
    if (email.send(true))
    {
      set_alert("success", "Seems like your SMTP settings is set correctly. Check your email now.");
      hooks.do_action("smtp_test_email_success");
    }
    else
    {
      set_debug_alert("<h1>Your SMTP settings are not set correctly here is the debug log.</h1><br />" + email.print_debugger() + (string.IsNullOrEmpty(globals<string>("debug")) ? globals<string>("debug") : ""));
      hooks.do_action("smtp_test_email_failed");
    }

    return Ok();
  }
}
