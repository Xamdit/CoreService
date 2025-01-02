using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Framework.Helpers;
using Service.Helpers;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(ILogger<ConsentController> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  public override void Init()
  {
    hooks.do_action("clients_authentication_constructor", this);
  }

  [HttpGet]
  public IActionResult index()
  {
    return login();
  }

  // Added for backward compatibilies
  public IActionResult admin()
  {
    return Redirect(admin_url("authentication"));
  }

  public IActionResult login()
  {
    if (db.is_client_logged_in()) return Redirect(site_url());
    form_validation.set_rules("password", self.helper.label("clients_login_password"), "required");
    form_validation.set_rules("email", self.helper.label("clients_login_email"), "trim|required|valid_email");

    if (db.get_option_compare("use_recaptcha_customers_area", 1)
        && !string.IsNullOrEmpty(db.get_option("recaptcha_secret_key"))
        && !string.IsNullOrEmpty(db.get_option("recaptcha_site_key")))
      form_validation.set_rules("g-recaptcha-response", "Captcha", "callback_recaptcha");
    if (form_validation.run())
    {
      var authentication_model = self.authentication_model(db);
      var result = authentication_model.login(
        self.input.post<string>("email"),
        self.input.post<string>("password"),
        self.input.post<bool>("remember"),
        false
      );

      if (result.is_success && result.memberinactive)
      {
        set_alert("danger", self.helper.label("inactive_account"));
        return Redirect(self.helper.site_url("authentication/login"));
      }

      if (result.is_success)
      {
        set_alert("danger", self.helper.label("client_invalid_username_or_password"));
        return Redirect(self.helper.site_url("authentication/login"));
      }

      var announcements_model = self.announcements_model(db);
      announcements_model.set_announcements_as_read_except_last_one(self.helper.get_contact_user_id());
      hooks.do_action("after_contact_login");
      this.maybe_redirect_to_previous_url();
      return Redirect(site_url());
    }

    data.title = self.helper.label(
      db.get_option_compare("allow_registration", 1)
        ? "clients_login_heading_register"
        : "clients_login_heading_no_register"
    );
    data.bodyclass = "customers_login";
    // data(data);
    // this.view("login");
    // this.layout();
    return Ok();
  }

  [HttpGet("register")]
  public IActionResult register()
  {
    var clients_model = self.clients_model(db);
    var authentication_model = self.authentication_model(db);
    if (!db.get_option_compare("allow_registration", 1) || db.is_client_logged_in())
      return Redirect(site_url());
    if (db.get_option_compare("company_is_required", 1))
      form_validation.set_rules("company", self.helper.label("client_company"), "required");

    if (self.helper.is_gdpr() && db.get_option_compare("gdpr_enable_terms_and_conditions", 1))
      form_validation.set_rules(
        "accept_terms_and_conditions",
        self.helper.label("terms_and_conditions"),
        "required",
        new Dictionary<string, string> { { "required", self.helper.label("terms_and_conditions_validation") } }
      );

    form_validation.set_rules("firstname", self.helper.label("client_firstname"), "required");
    form_validation.set_rules("lastname", self.helper.label("client_lastname"), "required");
    form_validation.set_rules("email", self.helper.label("client_email"), "trim|required|is_unique[contacts.email]|valid_email");
    form_validation.set_rules("password", self.helper.label("clients_register_password"), "required");
    form_validation.set_rules("passwordr", self.helper.label("clients_register_password_repeat"), "required|matches[password]");

    if (db.get_option_compare("use_recaptcha_customers_area", 1)
        && db.get_option("recaptcha_secret_key") != ""
        && db.get_option("recaptcha_site_key") != "")
      form_validation.set_rules("g-recaptcha-response", "Captcha", "callback_recaptcha");

    var custom_fields = db.get_custom_fields("customers", x => x.ShowOnClientPortal && x.Required == 1);
    var custom_fields_contacts = db.get_custom_fields("contacts", x => x.ShowOnClientPortal && x.Required == 1);

    foreach (var field in custom_fields)
    {
      var field_name = "custom_fields[" + field.FieldTo + "][" + field.Id + "]";
      if (field.Type == "checkbox" || field.Type == "multiselect") field_name += "[]";
      form_validation.set_rules(field_name, field.Name, "required");
    }

    foreach (var field in custom_fields_contacts)
    {
      var field_name = "custom_fields[" + field.FieldTo + "][" + field.Id + "]";
      if (field.Type == "checkbox" || field.Type == "multiselect") field_name += "[]";
      form_validation.set_rules(field_name, field.Name, "required");
    }

    data.title = self.helper.label("clients_register_heading");
    data.bodyclass = "register";
    // data(data);
    // this.view("register");
    // this.layout();
    return Ok();
  }

  [HttpPost("register")]
  public IActionResult register_post([FromForm] string email, [FromForm] string password)
  {
    var clients_model = self.clients_model(db);
    var authentication_model = self.authentication_model(db);
    if (!db.get_option_compare("allow_registration", 1) || db.is_client_logged_in())
      return Redirect(site_url());
    if (db.get_option_compare("company_is_required", 1))
      form_validation.set_rules("company", self.helper.label("client_company"), "required");

    if (self.helper.is_gdpr() && db.get_option_compare("gdpr_enable_terms_and_conditions", 1))
      form_validation.set_rules(
        "accept_terms_and_conditions",
        self.helper.label("terms_and_conditions"),
        "required",
        new Dictionary<string, string> { { "required", self.helper.label("terms_and_conditions_validation") } }
      );

    form_validation.set_rules("firstname", self.helper.label("client_firstname"), "required");
    form_validation.set_rules("lastname", self.helper.label("client_lastname"), "required");
    form_validation.set_rules("email", self.helper.label("client_email"), "trim|required|is_unique[contacts.email]|valid_email");
    form_validation.set_rules("password", self.helper.label("clients_register_password"), "required");
    form_validation.set_rules("passwordr", self.helper.label("clients_register_password_repeat"), "required|matches[password]");

    if (db.get_option_compare("use_recaptcha_customers_area", 1)
        && db.get_option("recaptcha_secret_key") != ""
        && db.get_option("recaptcha_site_key") != "")
      form_validation.set_rules("g-recaptcha-response", "Captcha", "callback_recaptcha");

    var custom_fields = db.get_custom_fields("customers", x => x.ShowOnClientPortal && x.Required == 1);
    var custom_fields_contacts = db.get_custom_fields("contacts", x => x.ShowOnClientPortal && x.Required == 1);

    foreach (var field in custom_fields)
    {
      var field_name = "custom_fields[" + field.FieldTo + "][" + field.Id + "]";
      if (field.Type == "checkbox" || field.Type == "multiselect") field_name += "[]";
      form_validation.set_rules(field_name, field.Name, "required");
    }

    foreach (var field in custom_fields_contacts)
    {
      var field_name = "custom_fields[" + field.FieldTo + "][" + field.Id + "]";
      if (field.Type == "checkbox" || field.Type == "multiselect") field_name += "[]";
      form_validation.set_rules(field_name, field.Name, "required");
    }


    data = self.input.post<dynamic>();
    self.helper.define("CONTACT_REGISTERING", true);
    var client_id = clients_model.add(new Client()
    {
      BillingStreet = data.address,
      BillingCity = data.city,
      BillingState = data.state,
      BillingZip = data.zip
      // BillingCountry = is_numeric(data.country) ? data.country : 0,
      // "firstname" => data.firstname,
      // "lastname" => data.lastname,
      // "email" => data.email,
      // "contact_phonenumber" => data.contact_phonenumber,
      // "website" => data.website,
      // "title" => data.title,
      // "password" => data.passwordr,
      // "company" => data.company,
      // "vat" => isset(data.vat) ? data.vat : "",
      // "phonenumber" => data.phonenumber,
      // "country" => data.country,
      // "city" => data.city,
      // "address" => data.address,
      // "zip" => data.zip,
      // "state" => data.state,
      // "custom_fields" => isset(data.custom_fields) && is_array(data.custom_fields) ? data.custom_fields : []
    });


    if (client_id <= 0) return Ok();
    hooks.do_action("after_client_register", client_id);
    if (db.get_option("customers_register_require_confirmation") == "1")
    {
      self.helper.send_customer_registered_email_to_administrators(client_id);
      clients_model.require_confirmation(client_id);
      set_alert("success", self.helper.label("customer_register_account_confirmation_approval_notice"));
      return Redirect(self.helper.site_url("authentication/login"));
    }

    var result = authentication_model.login(email, password, false, false);
    var redUrl = site_url();

    if (result.is_success)
    {
      hooks.do_action("after_client_register_logged_in", client_id);
      set_alert("success", self.helper.label("clients_successfully_registered"));
    }
    else
    {
      set_alert("warning", self.helper.label("clients_account_created_but_not_logged_in"));
      redUrl = self.helper.site_url("authentication/login");
    }

    self.helper.send_customer_registered_email_to_administrators(client_id);
    return Redirect(redUrl);
  }

  [HttpGet("forgot_password")]
  public IActionResult forgot_password_get()
  {
    var authentication_model = self.authentication_model(db);
    if (db.is_client_logged_in()) return Redirect(site_url());

    form_validation.set_rules(
      "email",
      self.helper.label("customer_forgot_password_email"),
      "trim|required|valid_email|callback_contact_email_exists"
    );

    data.title = self.helper.label("customer_forgot_password");
    // data(data);
    // this.view("forgot_password");
    // this.layout();
    return Ok();
  }

  [HttpPost("forgot_password")]
  public IActionResult forgot_password([FromForm] string email)
  {
    var authentication_model = self.authentication_model(db);
    if (db.is_client_logged_in()) return Redirect(site_url());

    form_validation.set_rules(
      "email",
      self.helper.label("customer_forgot_password_email"),
      "trim|required|valid_email|callback_contact_email_exists"
    );


    var result = authentication_model.forgot_password(email);
    if (result.is_success && result.memberinactive)
      set_alert("danger", self.helper.label("inactive_account"));
    else if (result.is_success)
      set_alert("success", self.helper.label("check_email_for_resetting_password"));
    else
      set_alert("danger", self.helper.label("error_setting_new_password_key"));
    return Redirect(self.helper.site_url("authentication/forgot_password"));
  }

  [HttpGet("reset_password")]
  public IActionResult reset_password(bool is_staff, int userid, string new_pass_key)
  {
    var authentication_model = self.authentication_model(db);
    if (!authentication_model.can_reset_password(is_staff, userid, new_pass_key))
    {
      set_alert("danger", self.helper.label("password_reset_key_expired"));
      return Redirect(self.helper.site_url("authentication/login"));
    }

    form_validation.set_rules("password", self.helper.label("customer_reset_password"), "required");
    form_validation.set_rules("passwordr", self.helper.label("customer_reset_password_repeat"), "required|matches[password]");
    data.title = self.helper.label("admin_auth_reset_password_heading");
    // data(data);
    // this.view("reset_password");
    // this.layout();
    return Ok();
  }


  [HttpPost("reset_password")]
  public IActionResult reset_password_post(bool is_staff, int userid, string new_pass_key)
  {
    var passwordr = string.Empty;

    var authentication_model = self.authentication_model(db);
    if (!authentication_model.can_reset_password(is_staff, userid, new_pass_key))
    {
      set_alert("danger", self.helper.label("password_reset_key_expired"));
      return Redirect(self.helper.site_url("authentication/login"));
    }

    form_validation.set_rules("password", self.helper.label("customer_reset_password"), "required");
    form_validation.set_rules("passwordr", self.helper.label("customer_reset_password_repeat"), "required|matches[password]");
    hooks.do_action("before_user_reset_password", new { is_staff, userid });
    var staff = new { };
    var result = authentication_model.reset_password(false, userid, new_pass_key, passwordr);
    if (result.is_success && result.expired)
    {
      set_alert("danger", self.helper.label("password_reset_key_expired"));
    }
    else if (result.is_success)
    {
      hooks.do_action("after_user_reset_password", new { staff, userid });
      set_alert("success", self.helper.label("password_reset_message"));
    }
    else
    {
      set_alert("danger", self.helper.label("password_reset_message_fail"));
    }

    return Redirect(self.helper.site_url("authentication/login"));
  }

  public IActionResult logout()
  {
    var authentication_model = self.authentication_model(db);
    authentication_model.logout(false);
    hooks.do_action("after_client_logout");
    return Redirect(self.helper.site_url("authentication/login"));
  }

  public bool contact_email_exists(string email = "")
  {
    var total_rows = db.Contacts.Count(x => x.Email == email);
    if (total_rows != 0) return true;
    form_validation.set_message("contact_email_exists", self.helper.label("auth_reset_pass_email_not_found"));
    return false;
  }

  public async Task<IActionResult> recaptcha(string str = "")
  {
    return await this.do_recaptcha_validation(str);
  }
}
