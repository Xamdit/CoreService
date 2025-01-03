using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Helpers;
using Service.Models.Gdpr;

namespace Service.Controllers.Admin.Gdpr;

public class GdprController(ILogger<GdprController> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{
  public List<string> notAdminAllowed { get; set; }
  public GdprModel gdpr_model { get; set; }

  public override void Init()
  {
    notAdminAllowed = new List<string>() { "lead_consent_opt_action", "contact_consent_opt_action" };
    // if (!db.is_admin() && !in_array(self.uri.segment(3), notAdminAllowed)) access_denied("GDPR");
    gdpr_model = self.gdpr_model(db);
  }

  public IActionResult index()
  {
    data.page = self.input.get_has("page") ? self.input.get("page") : "general";
    data.save = true;
    if (data.page == "forgotten")
    {
      data.requests = gdpr_model.get_removal_requests();
      data.not_pending_requests = db.GdprRequests.Count(x => x.Status == "pending");
    }
    else if (data.page == "consent")
    {
      data.consent_purposes = gdpr_model.get_consent_purposes();
    }

    data.title = label("gdpr");
    return MakeSuccess(data);
  }

  public IActionResult save()
  {
    var page = self.input.get_has("page") ? self.input.get("page") : "general";
    data = self.input.post("settings");
    //XSS filtered from tinymce
    List<string> noXSS = ["terms_and_conditions", "privacy_policy", "gdpr_consent_public_page_top_block", "gdpr_page_top_information_block"];

    if (page == "portability")
    {
      data.gdpr_lead_data_portability_allowed = isset(data, "gdpr_lead_data_portability_allowed") ? data.gdpr_lead_data_portability_allowed : new List<object>();
      data.gdpr_lead_data_portability_allowed = JsonConvert.SerializeObject(data.gdpr_lead_data_portability_allowed);
      data.gdpr_contact_data_portability_allowed = isset(data, "gdpr_contact_data_portability_allowed") ? data.gdpr_contact_data_portability_allowed : new List<object>();
      data.gdpr_contact_data_portability_allowed = JsonConvert.SerializeObject(data.gdpr_contact_data_portability_allowed);
    }

    foreach (var desc in TypeDescriptor.GetProperties(data))
    {
      if (noXSS.Contains(desc.Name))
      {
        var settings = self.input.post<Dictionary<string, object>>("settings");
        desc.SetValue(data, settings[desc.Name]);
      }

      // db.update_option( desc.Name, (object)desc.GetValue(data));
      SettingHelper.update_option(db, desc.Name, desc.GetValue(data));
    }


    return Redirect(admin_url("gdpr/index?page=" + page));
  }

  public async Task<IActionResult> change_removal_request_status(int id, int status)
  {
    var request = new GdprRequest()
    {
      Status = $"{status}"
    };
    var is_success = await gdpr_model.update(id, request);
    if (is_success) set_alert("success", label("updated_successfully", label("removal_request")));

    return Ok();
  }

  [HttpPost("consent-purpose/{id:int?}")]
  public IActionResult consent_purpose_get(int? id = null)
  {
    if (id.HasValue)
      data.purpose = gdpr_model.get_consent_purpose(id.Value);
    return MakeResult(data);
  }

  [HttpPost("consent-purpose/{id:int}")]
  public IActionResult consent_purpose([FromBody] ConsentPurpose schema, int? id = null)
  {
    schema.Description = Convert.ToString(schema.Description).nl2br();


    if (!id.HasValue)
    {
      var dataset = new ConsentPurpose()
      {
        Name = data.name,
        Description = data.description
      };
      gdpr_model.add_consent_purpose(dataset);
    }
    else
    {
      var dataset = new ConsentPurpose()
      {
        Description = data.description
      };

      if (string.IsNullOrEmpty(schema.Name))
        dataset.Name = schema.Name;
      gdpr_model.update_consent_purpose(id.Value, dataset);
    }

    return Redirect(admin_url("gdpr/index?page=consent"));
  }

  [HttpDelete("consent-purpose/{id:int}")]
  public IActionResult delete_consent_purpose(int? id)
  {
    gdpr_model.delete_consent_purpose(id.Value);
    return Redirect(admin_url("gdpr/index?page=consent"));
  }

  public IActionResult enable()
  {
    db.update_option("enable_gdpr", 1);
    return Redirect(admin_url("gdpr"));
  }

  [HttpPost("contact-consent-opt-action")]
  public IActionResult contact_consent_opt_action([FromBody] GdprSchema schema)
  {
    var contact_id = schema.contact_id;
    var client_id = db.get_user_id_by_contact_id(contact_id);

    if (!db.has_permission("customers", "", "view"))
      if (!db.is_customer_admin(client_id.Value))
        access_denied("Contact Consents Action");

    data = this.prepare_consent_opt_action_data(data);
    data.contact_id = contact_id;
    gdpr_model.add_consent(data);

    var referer = Request.Headers["Referer"].ToString();
    return Redirect(
      referer.Contains("all_contacts")
        ? admin_url("clients/all_contacts?&consents=" + contact_id)
        : admin_url("clients/client/" + client_id + "?group=contacts&consents=" + contact_id)
    );
  }

  [HttpPost("lead-consent-opt-action")]
  public IActionResult lead_consent_opt_action([FromBody] Consent schema)
  {
    var lead_id = schema.LeadId.Value;
    var leads_model = self.leads_model(db);

    if (!db.is_staff_member() || !leads_model.staff_can_access_lead(lead_id))
      return ajax_access_denied();

    data = this.prepare_consent_opt_action_data(data);
    data.lead_id = lead_id;
    gdpr_model.add_consent(data);
    return MakeResult(new { lead_id });
  }

  private Consent prepare_consent_opt_action_data(Consent data)
  {
    return new Consent()
    {
      Action = data.Action,
      PurposeId = data.PurposeId,
      Description = Convert.ToString(data.Description).nl2br(),
      OptInPurposeDescription = !string.IsNullOrEmpty(data.OptInPurposeDescription) ? data.OptInPurposeDescription.nl2br() : "",
      StaffName = db.get_staff_full_name()
    };
  }
}
