using Global.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Helpers;
using Service.Helpers.Tags;
using Service.Models.Tickets;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormsController(ILogger<FormsController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index()
  {
    return NotFound();
  }

  /**
   * Web to lead form
   * User no need to see anything like LEAD in the url, this is the reason the method is named wtl
   * @param  string key web to lead form key identifier
   * @return mixed
   */
  [HttpGet("wtl/{key}")]
  public IActionResult wtl(string key)
  {
    var leads_model = self.model.leads_model();


    var form = leads_model.get_form(x => x.FormKey == key);

    if (form == null) return NotFound();

    data.form_fields = JsonConvert.DeserializeObject(form.FormData);
    if (!data.form_fields) data.form_fields = new List<string>();
    if (self.input.post().ContainsKey("key"))
      if (self.input.post("key") == key)
      {
        var post_data = self.input.post();
        var required = new List<string>();

        foreach (var field in data.form_fields)
          if (isset(field.required))
            required.Add(field.Name);
        if (self.helper.is_gdpr() && db.get_option<int>("gdpr_enable_terms_and_conditions_lead_form") == 1)
          required.Add("accept_terms_and_conditions");

        foreach (var field in required)
          if (!isset(post_data[field]) || (isset(post_data[field]) && string.IsNullOrEmpty(post_data[field])))
          {
            this.output.set_status_header(422);
            return MakeError("This page is currently disabled, check back later.");
          }

        if (db.get_option("recaptcha_secret_key") != "" && db.get_option("recaptcha_site_key") != "" && form.recaptcha == 1)
          if (!self.helper.do_recaptcha_validation(post_data["gecaptcha-response"]))
            return MakeSuccess(new
            {
              success = false,
              message = self.helper.label("recaptcha_error")
            });


        var regular_fields = new Dictionary<string, string>();
        var custom_fields = new List<int>();
        foreach (var kvp in post_data)
        {
          var name = kvp.Name;
          var val = (int)kvp.Value;
          if (strpos(name, "form-cf-") != false)
          {
            custom_fields.Add(new
            {
              name = name,
              value = val
            });
          }
          else
          {
            if (!db.field_exists(name, "leads")) continue;
            if (name == "country")
            {
              if (!is_numeric(val))
              {
                if (val == "")
                {
                  val = 0;
                }
                else
                {
                  var country = self.db().Countries.FirstOrDefault(x => x.Iso2 == val || x.ShortName == val || x.LongName == val);


                  val = country != null ? country.Id : 0;
                }
              }
            }
            else if (name == "address")
            {
              val = trim(val);
              val = nl2br(val);
            }

            regular_fields[name] = val;
          }
        }

        var success = false;
        var insert_to_db = true;

        if (form.AllowDuplicate == 0)
        {
          var where = CreateCondition<Lead>();
          if (!string.IsNullOrEmpty(form.TrackDuplicateField) && isset(regular_fields[form.TrackDuplicateField]))
            where[form.TrackDuplicateField] = regular_fields[form.TrackDuplicateField];
          if (!string.IsNullOrEmpty(form.TrackDuplicateFieldAnd) && regular_fields.ContainsKey("TrackDuplicateFieldAnd"))
            where[form.TrackDuplicateFieldAnd] = regular_fields[form.TrackDuplicateFieldAnd];

          if (count(where) > 0)
          {
            var total = self.db().Leads.Count(where);

            var duplicateLead = false;
            /**
                       * Check if the lead is only 1 time duplicate
                       * Because we wont be able to know how user is tracking duplicate and to send the email template for
                       * the request
                       */
            if (total == 1)
            {
              db.where(where);
              duplicateLead = db.get("leads").row();
            }

            if (total > 0)
            {
              // Success set to true for the response.
              success = true;
              insert_to_db = false;
              if (form.create_task_on_duplicate == 1)
              {
                var task_name_from_form_name = false;
                var task_name = "";
                if (isset(regular_fields.Name))
                {
                  task_name = regular_fields.Name;
                }
                else if (isset(regular_fields.email))
                {
                  task_name = regular_fields.email;
                }
                else if (isset(regular_fields.company))
                {
                  task_name = regular_fields.company;
                }
                else
                {
                  task_name_from_form_name = true;
                  task_name = form.Name;
                }

                if (task_name_from_form_name == false) task_name += " - " + form.Name;

                var description = "";
                var custom_fields_parsed = [];
                foreach (custom_fields as key => field) {
                  custom_fields_parsed[field["name"]] = field["value"];
                }

                var all_fields = array_merge(regular_fields, custom_fields_parsed);
                var fields_labels = new List<string>();
                foreach (var f in data.form_fields)
                  if (f.type != "header" && f.type != "paragraph" && f.type != "file")
                    fields_labels[f.Name] = f.label;

                description += form.Name + "<br /><br />";
                foreach (all_fields as name => val) {
                  if (isset(fields_labels[name]))
                  {
                    if (name == "country" && is_numeric(val))
                    {
                      c = get_country(val);
                      if (c)
                        val = c.short_name;
                      else
                        val = "Unknown";
                    }

                    description += fields_labels[name] + ": " + val + "<br />";
                  }
                }

                task_data =
                [
                  name = task_name,
                  priority = db.get_option("default_task_priority"),
                  dateadded = date("Y-m-d H:i:s"),
                  startdate = date("Y-m-d"),
                  addedfrom = form.responsible,
                  status = 1,
                  description = description
                ];

                task_data = self.hooks.apply_filters("before_add_task", task_data);
                db.insert("tasks", task_data);
                task_id = db.insert_id();
                if (task_id)
                {
                  attachment = handle_task_attachments_array(task_id, "file-input");

                  if (attachment && count(attachment) > 0) this.tasks_model.add_attachment_to_database(task_id, attachment, false, false);

                  assignee_data =
                  [
                    "taskid" => task_id,
                    "assignee" => form.responsible
                  ];
                  this.tasks_model.add_task_assignees(assignee_data, true);

                  self.hooks.do_action("after_add_task", task_id);
                  if (duplicateLead && duplicateLead.email != "") send_mail_template("lead_web_form_submitted", duplicateLead);
                }
              }
            }
          }
        }

        if (insert_to_db == true)
        {
          regular_fields.status = form.lead_status;
          if ((isset(regular_fields.Name) && string.IsNullOrEmpty(regular_fields.Name)) || !isset(regular_fields.Name)) regular_fields.Name = "Unknown";
          regular_fields.source = form.lead_source;
          regular_fields.addedfrom = 0;
          regular_fields.lastcontact = null;
          regular_fields.assigned = form.responsible;
          regular_fields.dateadded = date("Y-m-d H:i:s");
          regular_fields.from_form_id = form.id;
          regular_fields.is_public = form.mark_public;
          db.insert("leads", regular_fields);
          var lead_id = db.insert_id();

          self.hooks.do_action("lead_created", new
          {
            lead_id = lead_id,
            web_to_lead_form = true
          });

          success = false;
          if (lead_id)
          {
            success = true;

            leads_model.log_lead_activity(lead_id, "not_lead_imported_from_form", true, serialize([
              form.Name
            ]));
            // /handle_custom_fields_post
            custom_fields_build["leads"] = [];
            foreach (custom_fields as cf) {
              cf_id = strafter(cf["name"], "form-cf-");
              custom_fields_build["leads"][cf_id] = cf["value"];
            }

            leads_model.lead_assigned_member_notification(lead_id, form.responsible, true);
            handle_custom_fields_post(lead_id, custom_fields_build);
            handle_lead_attachments(lead_id, "file-input", form.Name);
            var ids = new List<int>();
            if (form.notify_lead_imported != 0)
            {
              if (form.notify_type == "assigned")
              {
                to_responsible = true;
              }
              else
              {
                ids = @unserialize(form.notify_ids);
                to_responsible = false;
                if (form.notify_type == "specific_staff")
                  field = "staffid";
                else if (form.notify_type == "roles") field = "role";
              }

              var staff = new List<Staff>();
              if (to_responsible == false && is_array(ids) && count(ids) > 0)
                staff = db.Staff.Where(x => x.Active && ids.Contains(x.Id)).ToList();
              else
                staff =
                [
                  [
                    "staffid" => form.responsible
                  ]
                ];
              var notifiedUsers = [];
              foreach (staff as member) {
                if (member["staffid"] != 0)
                {
                  notified = add_notification([
                    "description" => "not_lead_imported_from_form",
                    "touserid" => member["staffid"],
                    "fromcompany" => 1,
                    "fromuserid" => null,
                    "additional_data" => serialize([
                    form.Name,
                    ]),
                    "link" => "#leadid=" + lead_id
                  ]);
                  if (notified) array_push(notifiedUsers, member["staffid"]);
                }
              }
              pusher_trigger_notification(notifiedUsers);
            }

            if (isset(regular_fields.email) && regular_fields.email != "")
            {
              lead = leads_model.get(lead_id);
              send_mail_template("lead_web_form_submitted", lead);
            }
          }
        } // end insert_to_db

        var lead_id = 0;
        var task_id = 0;
        if (success == true)
        {
          if (!isset(lead_id)) lead_id = 0;
          if (!isset(task_id)) task_id = 0;
          self.hooks.do_action("web_to_lead_form_submitted", new
          {
            lead_id = lead_id,
            form_id = form.Id,
            task_id = task_id
          });
        }

        return MakeSuccess(new
        {
          success = success,
          message = form.SuccessSubmitMsg
        });
      }

    data.form = form;
    // this.load.view("forms/web_to_lead", data);
    return MakeSuccess(data);
  }

  /**
   * Web to lead form
   * User no need to see anything like LEAD in the url, this is the reason the method is named eq lead
   * @param  string hash lead unique identifier
   * @return mixed
   */
  [HttpGet("l/{hash}")]
  public IActionResult l(string hash)
  {
    var gdpr_model = self.model.gdpr_model();
    var leads_model = self.model.leads_model();

    if (db.get_option("gdpr_enable_lead_public_form") == "0")
      return NotFound();

    var lead = leads_model.get(x => x.Hash == hash).FirstOrDefault();

    if (!lead || count(lead) > 1) return NotFound();

    lead = array_to_object(lead[0]);
    self.helper.load_lead_language(lead.Id);

    if (self.input.post().ContainsKey("update"))
    {
      data = self.input.post();
      unset(data.update);
      leads_model.update(data, lead.Id);
      return Redirect(_SERVER["HTTP_REFERER"]);
    }
    else if (self.input.post("export") && get_option("gdpr_data_portability_leads") == "1")
    {
      this.load.library("gdpr/gdpr_lead");
      this.gdpr_lead.export(lead.Id);
    }
    else if (self.input.post("removal_request"))
    {
      success = gdpr_model.add_removal_request(new
      {
        description = self.helper.nl2br(self.input.post("removal_description")),
        request_from = lead.Name,
        lead_id = lead.Id
      });
      if (success)
      {
        send_gdpr_email_template("gdpr_removal_request_by_lead", lead.Id);
        set_alert("success", self.helper.label("data_removal_request_sent"));
      }

      redirect(_SERVER["HTTP_REFERER"]);
    }

    lead.attachments = leads_model.get_lead_attachments(lead.Id);
    this.disableNavigation();
    this.disableSubMenu();
    data.title = lead.Name;
    data.lead = lead;
    this.view("forms/lead");
    data(data);
    this.layout(true);
    return MakeError();
  }

  [HttpGet("ticket")]
  public async Task<IActionResult> ticket()
  {
    var tickets_model = self.model.tickets_model();
    var form = new stdClass();
    form.language = db.get_option("active_language");
    form.recaptcha = 1;
    self.lang.load("ticket_form_lang", form.language);
    this.lang.load(form.language + "_lang", form.language);
    if (self.helper.file_exists(APPPATH + "language/" + form.language + "/custom_lang.php")) self.lang.load("custom_lang", form.language);

    form.SuccessSubmitMsg = self.helper.label("success_submit_msg");

    form = self.hooks.apply_filters("ticket_form_settings", form);

    if (self.input.post() && self.input.is_ajax_request())
    {
      var post_data = self.input.post();

      var required = ["subject", "department", "email", "name", "message", "priority"];

      if (self.helper.is_gdpr() && db.get_option("gdpr_enable_terms_and_conditions_ticket_form") == 1)
        required.Add("accept_terms_and_conditions");

      foreach (var field in required)
        if (!isset(post_data[field]) || (isset(post_data[field]) && string.IsNullOrEmpty(post_data[field])))
        {
          self.output.set_status_header(422);
          return MakeError();
        }

      if (db.get_option("recaptcha_secret_key") != "" && db.get_option("recaptcha_site_key") != "" && form.recaptcha == 1)
        if (!self.helper.do_recaptcha_validation(post_data["gecaptcha-response"]))
          return MakeSuccess(new
          {
            success = false,
            message = self.helper.label("recaptcha_error")
          });

      post_data = new
      {
        "email" => post_data.email,
        "name" => post_data.Name,
        "subject" => post_data.subject,
        "department" => post_data.department,
        "priority" => post_data.priority,
        "service" => isset(post_data.service) && is_numeric(post_data.service)
        ? post_data.service
        : null,
        "custom_fields" => isset(post_data.custom_fields) && is_array(post_data.custom_fields)
        ? post_data.custom_fields
        : [],
        "message" => post_data.message
      };

      var success = false;


      var result = self.db().Contacts.Where(x => x.Email == post_data.email).FirstOrDefault();

      if (result)
      {
        post_data.userid = result.userid;
        post_data.contactid = result.id;
        unset(post_data.email);
        unset(post_data.Name);
      }


      post_data = self.hooks.apply_filters("ticket_external_form_insert_data", post_data);
      var ticket_id = await tickets_model.add(convert<TicketOption>(post_data));
      if (ticket_id == 0) success = true;

      if (success == true)
        self.hooks.do_action("ticket_form_submitted", new
        {
          ticket_id = ticket_id
        });

      return MakeSuccess(new
      {
        success = success,
        message = form.SuccessSubmitMsg
      });
    }


    var departments_model = self.model.departments_model();

    data.departments = departments_model.get();
    data.priorities = tickets_model.get_priority();

    data.priorities["callback_translate"] = "ticket_priority_translate";
    data.services = tickets_model.get_service();

    data.form = form;
    // this.load.view("forms/ticket", data);
  }
}
