using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsentController(ILogger<ConsentController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult Index()
  {
    return NotFound();
  }

  [HttpPost("contact")]
  public IActionResult Contact([FromForm] string key)
  {
    var (self, db) = getInstance();
    var clientsModel = self.model.clients_model();

    if ((self.helper.is_gdpr() && db.get_option_compare("gdpr_enable_consent_for_contacts", "0")) || !self.helper.is_gdpr())
      return MakeError("This page is currently disabled, check back later.");

    var meta = db.UserMeta.FirstOrDefault(x => x.MetaKey == "consent_key" && x.MetaValue == key);

    if (meta == null)
      return NotFound();

    var contact = clientsModel.get_contact(meta.ContactId);

    if (contact == null)
      return NotFound();

    var gdpr_model = self.model.gdpr_model();

    if (Request.Method == "POST" && Request.Form.Any())
    {
      foreach (var purposeId in Request.Form.Keys)
        if (int.TryParse(purposeId, out var parsedPurposeId))
        {
          var action = Request.Form[purposeId];
          var purpose = gdpr_model.get_consent_purpose(parsedPurposeId);
          if (purpose != null)
            gdpr_model.add_consent(new Consent
            {
              Action = action,
              PurposeId = parsedPurposeId,
              ContactId = contact.Id,
              Description = "Consent Updated From Web Form",
              OptInPurposeDescription = purpose.Description
            });
        }

      return Redirect(Request.Headers["Referer"].ToString());
    }

    var data = new
    {
      contact,
      purposes = gdpr_model.get_consent_purposes(contact.Id, "contact"),
      title = self.helper.label("gdpr") + " - " + contact.FirstName + " " + contact.LastName,
      bodyclass = "consent"
    };

    this.data(data);
    // this.view("consent");
    // no_index_customers_area();
    // this.disableNavigation();
    // this.disableSubMenu();
    // return this.layout();
    return Ok();
  }

  [HttpPost("lead")]
  public IActionResult Lead([FromForm] string hash)
  {
    var (self, db) = getInstance();
    var gdpr_model = self.model.gdpr_model();

    if ((self.helper.is_gdpr() && db.get_option("gdpr_enable_consent_for_leads") == "0") || !self.helper.is_gdpr())
      return MakeError("This page is currently disabled, check back later.");

    var lead = db.Leads.FirstOrDefault(x => x.Hash == hash);

    if (lead == null)
      return NotFound();

    if (Request.Method == "POST" && Request.Form.Any())
    {
      foreach (var purposeId in Request.Form.Keys)
        if (int.TryParse(purposeId, out var parsedPurposeId))
        {
          var action = Request.Form[purposeId];
          var purpose = gdpr_model.get_consent_purpose(parsedPurposeId);
          if (purpose != null)
            gdpr_model.add_consent(new Consent()
            {
              Action = action,
              PurposeId = parsedPurposeId,
              LeadId = lead.Id,
              Description = "Consent Updated From Web Form",
              OptInPurposeDescription = purpose.Description
            });
        }

      return Redirect(Request.Headers["Referer"].ToString());
    }

    var data = new
    {
      lead,
      purposes = gdpr_model.get_consent_purposes(lead.Id, "lead"),
      title = self.helper.label("gdpr") + " - " + lead.Name,
      bodyclass = "consent"
    };

    this.data(data);
    // this.view("consent");
    // no_index_customers_area();
    // this.disableNavigation();
    // this.disableSubMenu();
    // return this.layout();
    return Ok();
  }
}
