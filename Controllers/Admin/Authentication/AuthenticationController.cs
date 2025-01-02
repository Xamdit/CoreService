using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Libraries.FormValidations;

namespace Service.Controllers.Admin.Authentication;

[ApiController]
[Route("api/admin/authentication")]
public class AuthenticationController(ILogger<AuthenticationController> logger, MyInstance self, MyContext db) : AppControllerBase(logger, self, db)
{
  public override void Init()
  {
    // if (this.app.is_db_upgrade_required()) HttpContext.Response.return Redirect(admin_url());
    // db.load_admin_language();
    var authentication_model = self.authentication_model(db);
    self.library.form_validation().set_rules("required", label("form_validation_required"));
    self.library.form_validation().set_rules("valid_email", label("form_validation_valid_email"));
    self.library.form_validation().set_rules("matches", label("form_validation_matches"));
    hooks.do_action("admin_auth_init");
  }

  public IActionResult index()
  {
    return admin();
  }

  [HttpGet]
  public IActionResult admin()
  {
    var form_validation = self.library.form_validation();
    if (db.is_staff_logged_in())
      return Redirect(admin_url());
    form_validation.set_rules("password", label("admin_auth_login_password"), "required");
    form_validation.set_rules("email", label("admin_auth_login_email"), "trim|required|valid_email");
    if (db.get_option("recaptcha_secret_key") != "" && db.get_option("recaptcha_site_key") != "") form_validation.set_rules("g-recaptcha-response", "Captcha", "callback_recaptcha");
    data.title = label("admin_auth_login_heading");
    return Ok();
  }

  [HttpPost]
  public IActionResult admin([FromBody] AuthSchema schema)
  {
    var form_validation = self.library.form_validation();
    if (db.is_staff_logged_in())
      return Redirect(admin_url());
    form_validation.set_rules("password", label("admin_auth_login_password"), "required");
    form_validation.set_rules("email", label("admin_auth_login_email"), "trim|required|valid_email");
    if (db.get_option("recaptcha_secret_key") != "" && db.get_option("recaptcha_site_key") != "") form_validation.set_rules("g-recaptcha-response", "Captcha", "callback_recaptcha");
    var authentication_model = self.authentication_model(db);
    if (form_validation.run())
    {
      var result = authentication_model.login(schema.email, schema.password, schema.remember, true);
      if (result.is_success && data.memberinactive != null)
      {
        set_alert("danger", label("admin_auth_inactive_account"));
        return Redirect(admin_url("authentication"));
      }

      if (result.is_success && data.two_factor_auth != null)
      {
        authentication_model.set_two_factor_auth_code(data.user.staffid);
        var sent = db.send_mail_template("staff_two_factor_auth_key", $"{data.user}");
        if (!sent)
        {
          set_alert("danger", label("two_factor_auth_failed_to_send_code"));
          return Redirect(admin_url("authentication"));
        }

        set_alert("success", label("two_factor_auth_code_sent_successfully", schema.email));
        return Redirect(admin_url("authentication/two_factor"));
      }
    }

    if (data == false)
    {
      set_alert("danger", label("admin_auth_invalid_email_or_password"));
      return Redirect(admin_url("authentication"));
    }

    var announcements_model = self.announcements_model(db);
    announcements_model.set_announcements_as_read_except_last_one(db.get_staff_user_id(), true);
    // is logged in
    this.maybe_redirect_to_previous_url();
    hooks.do_action("after_staff_login");
    return Redirect(admin_url());
  }

  [HttpPost]
  public IActionResult two_factor()
  {
    var form_validation = self.library.form_validation();
    form_validation.set_rules("code", label("two_factor_authentication_code"), "required");
    var authentication_model = self.authentication_model(db);
    if (form_validation.run() == false) return Ok();
    var code = self.input.post<string>("code");
    code = code.Trim();
    if (authentication_model.is_two_factor_code_valid(code))
    {
      var user = authentication_model.get_user_by_two_factor_auth_code(code);
      authentication_model.clear_two_factor_auth_code(user.Id);
      authentication_model.two_factor_auth_login(user);
      var announcements_model = self.announcements_model(db);
      announcements_model.set_announcements_as_read_except_last_one(db.get_staff_user_id(), true);
      this.maybe_redirect_to_previous_url();
      hooks.do_action("after_staff_login");
      return Redirect(admin_url());
    }

    set_alert("danger", label("two_factor_code_not_valid"));
    return Redirect(admin_url("authentication/two_factor"));
  }

  [HttpGet("forgot-password")]
  public IActionResult forgot_password()
  {
    if (db.is_staff_logged_in())
      return Redirect(admin_url());
    var form_validation = self.library.form_validation();
    form_validation.set_rules("email", label("admin_auth_login_email"), "trim|required|valid_email|callback_email_exists");
    return Ok();
  }

  [HttpPost("forgot-password")]
  public IActionResult forgot_password_post()
  {
    if (db.is_staff_logged_in())
      return Redirect(admin_url());
    var authentication_model = self.authentication_model(db);
    var form_validation = self.library.form_validation();
    form_validation.set_rules("email", label("admin_auth_login_email"), "trim|required|valid_email|callback_email_exists");

    if (form_validation.run())
    {
      var result = authentication_model.forgot_password(self.input.post("email"), true);
      if (result.is_success && result.memberinactive)
      {
        set_alert("danger", label("inactive_account"));
        return Redirect(admin_url("authentication/forgot_password"));
      }
      else if (result.is_success)
      {
        set_alert("success", label("check_email_for_resetting_password"));
        return Redirect(admin_url("authentication"));
      }
      else
      {
        set_alert("danger", label("error_setting_new_password_key"));
        return Redirect(admin_url("authentication/forgot_password"));
      }
    }

    return Ok();
  }

  [HttpGet("reset-password")]
  public IActionResult reset_password(bool is_staff, int userid, string new_pass_key)
  {
    var authentication_model = self.authentication_model(db);
    if (!authentication_model.can_reset_password(is_staff, userid, new_pass_key))
    {
      set_alert("danger", label("password_reset_key_expired"));
      return Redirect(admin_url("authentication"));
    }

    var form_validation = self.library.form_validation();
    form_validation.set_rules("password", label("admin_auth_reset_password"), "required");
    form_validation.set_rules("passwordr", label("admin_auth_reset_password_repeat"), "required|matches[password]");
    return Ok();
  }

  [HttpGet("reset-password")]
  public IActionResult reset_password_get([FromBody] SetPasswordSchema schema)
  {
    var authentication_model = self.authentication_model(db);
    if (!authentication_model.can_reset_password(schema.is_staff, schema.user_id, schema.new_pass_key))
    {
      set_alert("danger", label("password_reset_key_expired"));
      return Redirect(admin_url("authentication"));
    }

    var form_validation = self.library.form_validation();
    form_validation.set_rules("password", label("admin_auth_reset_password"), "required");
    form_validation.set_rules("passwordr", label("admin_auth_reset_password_repeat"), "required|matches[password]");

    if (!form_validation.run()) return Redirect(admin_url("authentication"));
    hooks.do_action("before_user_reset_password", new
    {
      staff = schema.is_staff, schema.user_id
    });
    var result = authentication_model.reset_password(schema.is_staff, schema.user_id, schema.new_pass_key, schema.passwordr);
    if (result.is_success && result.expired)
    {
      set_alert("danger", label("password_reset_key_expired"));
    }
    else if (result.is_success)
    {
      hooks.do_action("after_user_reset_password", new { schema.is_staff, schema.user_id });
      set_alert("success", label("password_reset_message"));
    }
    else
    {
      set_alert("danger", label("password_reset_message_fail"));
    }

    return Redirect(admin_url("authentication"));
  }

  [HttpGet("set-password")]
  public IActionResult set_password_get([FromQuery] SetPasswordSchema schema)
  {
    var authentication_model = self.authentication_model(db);
    if (!authentication_model.can_set_password(schema.is_staff, schema.user_id, schema.new_pass_key))
    {
      set_alert("danger", label("password_reset_key_expired"));
      return Redirect(schema.is_staff ? admin_url("authentication") : site_url());
    }

    var form_validation = self.library.form_validation();
    form_validation.set_rules("password", label("admin_auth_set_password"), "required");
    form_validation.set_rules("passwordr", label("admin_auth_set_password_repeat"), "required|matches[password]");
    if (!form_validation.run()) return Ok();
    var result = authentication_model.set_password(schema.is_staff, schema.user_id, schema.new_pass_key, schema.passwordr);
    if (result.is_success && result.expired == true)
      set_alert("danger", label("password_reset_key_expired"));
    else if (result.is_success == true)
      set_alert("success", label("password_reset_message"));
    else
      set_alert("danger", label("password_reset_message_fail"));
    return schema.is_staff
      ? Redirect(admin_url("authentication"))
      : Redirect(site_url());
  }

  [HttpPost("set-password")]
  public IActionResult set_password_post([FromBody] SetPasswordSchema schema)
  {
    var authentication_model = self.authentication_model(db);
    if (!authentication_model.can_set_password(schema.is_staff, schema.user_id, schema.new_pass_key))
    {
      set_alert("danger", label("password_reset_key_expired"));
      // return Redirect(admin_url("authentication"));
      return schema.is_staff
        ? Redirect(admin_url("authentication"))
        : Redirect(site_url());
    }

    var form_validation = self.library.form_validation();
    form_validation.set_rules("password", label("admin_auth_set_password"), "required");
    form_validation.set_rules("passwordr", label("admin_auth_set_password_repeat"), "required|matches[password]");
    if (!form_validation.run()) return Ok();
    var result = authentication_model.set_password(schema.is_staff, schema.user_id, schema.new_pass_key, schema.passwordr);
    if (result.is_success && result.expired == true)
      set_alert("danger", label("password_reset_key_expired"));
    else if (result.is_success)
      set_alert("success", label("password_reset_message"));
    else
      set_alert("danger", label("password_reset_message_fail"));
    return schema.is_staff
      ? Redirect(admin_url("authentication"))
      : Redirect(site_url());
  }

  [HttpPost("logout")]
  public IActionResult logout()
  {
    var authentication_model = self.authentication_model(db);
    authentication_model.logout();
    hooks.do_action("after_user_logout");
    return Redirect("authentication");
  }


  private bool email_exists(string email)
  {
    var total_rows = db.Staff.Count(x => x.Email == email);
    var form_validation = self.library.form_validation();
    if (total_rows != 0) return true;
    form_validation.set_message("email_exists", label("auth_reset_pass_email_not_found"));
    return false;
  }

  [HttpGet("recaptcha")]
  public async Task<IActionResult> recaptcha(string str = "")
  {
    return await this.do_recaptcha_validation(str);
  }
}
