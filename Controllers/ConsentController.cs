using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;
using Service.Helpers;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsentController(ILogger<MyControllerBase> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  [HttpGet]
  public IActionResult Index()
  {
    return NotFound();
  }

  [HttpPost("contact")]
  public IActionResult Contact([FromForm] string key)
  {
    var db = new MyContext();
    var clientsModel = self.clients_model(db);
    if ((db.is_gdpr() && db.get_option_compare("gdpr_enable_consent_for_contacts", "0")) || !db.is_gdpr())
      return MakeError("This page is currently disabled, check back later.");
    var meta = db.UserMeta.FirstOrDefault(x => x.MetaKey == "consent_key" && x.MetaValue == key);

    if (meta == null)
      return NotFound();

    var contact = clientsModel.get_contact(meta.ContactId);

    if (contact == null)
      return NotFound();

    var gdpr_model = self.gdpr_model(db);

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
      title = label("gdpr") + " - " + contact.FirstName + " " + contact.LastName,
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
    var db = new MyContext();
    var gdpr_model = self.gdpr_model(db);

    if ((db.is_gdpr() && db.get_option("gdpr_enable_consent_for_leads") == "0") || !db.is_gdpr())
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
      title = label("gdpr") + " - " + lead.Name,
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
