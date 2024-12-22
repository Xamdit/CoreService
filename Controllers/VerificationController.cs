using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController(ILogger<MyControllerBase> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult Index()
  {
    var (self, db) = getInstance();

    if (self.helper.is_contact_email_verified())
      return Redirect(self.helper.site_url("clients"));

    var data = new
    {
      title = self.helper.label("email_verification_required")
    };

    // this.view("verification_required");
    // this.data(data);
    // return this.layout();
    return Ok();
  }

  [HttpPost("verify")]
  public IActionResult Verify([FromForm] int id, [FromForm] string key)
  {
    var (self, db) = getInstance();
    var clientsModel = self.model.clients_model();
    var contact = clientsModel.get_contact(id);

    if (contact == null)
      return NotFound();

    if (!string.IsNullOrEmpty(contact.EmailVerifiedAt))
    {
      set_alert("info", self.helper.label("email_already_verified"));
      return Redirect(self.helper.site_url("clients"));
    }

    if (contact.EmailVerificationKey != key)
    {
      self.helper.show_error(self.helper.label("invalid_verification_key"));
      return MakeError(self.helper.label("invalid_verification_key"));
    }

    var timestampNowMinus2Days = DateTime.Now.AddDays(-2);
    var contactRegistered = DateTime.Parse(contact.EmailVerificationSentAt);

    if (contactRegistered < timestampNowMinus2Days)
    {
      self.helper.show_error(self.helper.label("verification_key_expired"));
      return MakeError(self.helper.label("verification_key_expired"));
    }

    clientsModel.mark_email_as_verified(contact.Id);

    if (db.Clients.Any(x => x.Id == contact.Id && x.RegistrationConfirmed == 0))
      set_alert("info", self.helper.label("email_successfully_verified_but_required_admin_confirmation"));
    else
      set_alert("success", self.helper.label("email_successfully_verified"));

    var redirectUri = is_client_logged_in() ? "clients" : "authentication";
    return Redirect(self.helper.site_url(redirectUri));
  }

  [HttpGet("resend")]
  public IActionResult Resend()
  {
    var (self, db) = getInstance();
    var clientsModel = self.model.clients_model();

    if (self.helper.is_contact_email_verified() || !is_client_logged_in())
      return Redirect(self.helper.site_url("clients"));

    if (clientsModel.send_verification_email(self.helper.get_contact_user_id()))
      set_alert("success", self.helper.label("email_verification_mail_sent_successfully"));
    else
      set_alert("danger", self.helper.label("failed_to_send_verification_email"));

    return Redirect(self.helper.site_url("verification"));
  }
}
