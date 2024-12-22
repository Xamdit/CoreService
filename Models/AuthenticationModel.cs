using System.Dynamic;
using Global.Entities;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Helpers;
using Task = System.Threading.Tasks.Task;

namespace Service.Models;

public class AuthenticationModel(MyInstance self, MyContext db) : MyModel(self)
{
  public UserAutoLoginModel user_autologin = self.model.user_autologin();

  public void GoUpdate<T>(T entity) where T : class
  {
    // Access the DbSet for the given type T
    var dbSet = db.Set<T>();
    dbSet.Update(entity);
    db.SaveChanges();
  }

  /**
   * @param  string Email address for login
   * @param  string User Password
   * @param  boolean Set cookies for user if remember me is checked
   * @param  boolean Is Staff Or Client
   * @return boolean if not redirect url found, if found redirect to the url
   */
  public dynamic login(string email, string password, bool remember, bool is_staff)
  {
    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return false;
    var jsonString = is_staff
      ? JsonConvert.SerializeObject(db.Staff.FirstOrDefault(x => x.Email == email))
      : JsonConvert.SerializeObject(db.Contacts.FirstOrDefault(x => x.Email == email));


    var user = JsonConvert.DeserializeObject<Contact>(jsonString);
    if (user != null)
    {
      // Email is okey lets check the password now
      if (!self.VerifyPassword(password, user.Password))
      {
        self.hooks.do_action("failed_login_attempt", new { user, is_staff_member = is_staff });
        log_activity($"Failed Login Attempt [Email: {email}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
        // Password failed, return
        return false;
      }
    }
    else
    {
      self.hooks.do_action("non_existent_user_login_attempt", new
      {
        email,
        is_staff_member = is_staff
      });

      log_activity($"Non Existing User Tried to Login [Email: {email}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
      return false;
    }

    if (!user.Active)
    {
      self.hooks.do_action("inactive_user_login_attempt", new { user, is_staff_member = is_staff });
      log_activity($"Inactive User Tried to Login [Email: {email}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
      return new { memberinactive = true };
    }

    var twoFactorAuth = 0;
    if (is_staff)
    {
      var staff = JsonConvert.DeserializeObject<Staff>(jsonString);
      if (staff == null) return false;
      twoFactorAuth = staff.TwoFactorAuthEnabled;
      if (twoFactorAuth > 0)
      {
        self.hooks.do_action("before_staff_login", new { email, user_id = staff.Id });
        dynamic user_data = new
        {
          staff_user_id = staff.Id,
          staff_logged_in = true
        };
      }
      else
      {
        dynamic user_data = new ExpandoObject();
        user_data.tfa_staffid = staff.Id;
        if (remember) user_data.tfa_remember = true;
      }
    }
    else
    {
      self.hooks.do_action("before_client_login", new
      {
        email,
        user_id = user.UserId,
        contact_user_id = user.Id
      });

      var user_data = new
      {
        client_user_id = user.UserId,
        contact_user_id = user.Id,
        client_logged_in = true
      };
    }

    // this.session.set_userdata(user_data);

    if (twoFactorAuth > 0)
    {
      if (remember) create_autologin(user.Id, is_staff);
      update_login_info(user.Id, is_staff);
    }
    else
    {
      return new { two_factor_auth = true, user };
    }

    return true;
  }

  /**
   * @param  boolean If Client or Staff
   * @return none
   */
  public async Task logout(bool staff = true)
  {
    delete_autologin(staff);
    var isClientLoggedIn = self.helper.is_client_logged_in();
    if (isClientLoggedIn)
      self.hooks.do_action("before_contact_logout", self.helper.get_client_user_id());
    // this.session.unset_userdata("client_user_id");
    // this.session.unset_userdata("client_logged_in");
    else
      self.hooks.do_action("before_staff_logout", self.helper.get_staff_user_id());
    // this.session.unset_userdata("staff_user_id");
    // this.session.unset_userdata("staff_logged_in");
    // this.session.sess_destroy();
  }

  /**
   * @param  integer ID to create autologin
   * @param  boolean Is Client or Staff
   * @return boolean
   */
  private bool create_autologin(int user_id, bool is_staff)
  {
    // var key = substr(md5(uniqid(rand().get_cookie(this.config.item("sess_cookie_name")))), 0, 16);
    var key = "sess_cookie_name";

    user_autologin.delete(user_id, key, is_staff);
    if (!user_autologin.set(user_id, self.helper.md5(key), is_staff)) return false;
    self.input.cookies.set_cookie(new
    {
      name = "autologin",
      value = JsonConvert.SerializeObject(new { user_id, key }),
      expire = 60 * 60 * 24 * 31 * 2 // 2 months
    });

    return true;
  }

  /**
   * @param  boolean Is Client or Staff
   * @return none
   */
  private void delete_autologin(bool staff)
  {
    // if (cookie = get_cookie("autologin", true)) {
    //   data = unserialize(cookie);
    //   this.user_autologin.delete(data["user_id"], md5(data["key"]), staff);
    //   delete_cookie("autologin", "aal");
    // }
  }

  /**
   * @return boolean
   * Check if autologin found
   */
  public async Task<bool> autologin()
  {
    if (self.helper.is_logged_in()) return false;
    var cookie = self.input.cookies.get_cookie("autologin", true);
    if (cookie == null) return false;
    var data = JsonConvert.DeserializeObject<UserAutoLogin>(cookie);
    if (data == null) return false;
    if (!string.IsNullOrEmpty(data.Key) && data.UserId.HasValue) return false;
    var user = await user_autologin.get(data.UserId.Value, self.helper.md5(data.Key));
    if (user == null) return false;
    // Login user
    if (user.IsStaff)
    {
      var user_data = new
      {
        staff_user_id = user.Id,
        staff_logged_in = true
      };
    }
    else
    {
      // Get the customer id
      var contact = db.Contacts.FirstOrDefault(x => x.Id == user.Id);
      var user_data = new
      {
        client_user_id = contact.UserId,
        contact_user_id = user.Id,
        client_logged_in = true
      };
    }

    // this.session.set_userdata(user_data);
    // Renew users cookie to prevent it from expiring
    self.input.cookies.set_cookie(new
    {
      name = "autologin",
      value = cookie,
      expire = 60 * 60 * 24 * 31 * 2 // 2 months
    });
    update_login_info(user.Id, user.IsStaff);
    return true;
  }

  /**
   * @param  integer ID
   * @param  boolean Is Client or Staff
   * @return none
   * Update login info on autologin
   */
  private void update_login_info(int user_id, bool is_staff)
  {
    if (is_staff)
    {
      db.Staff
        .Where(x => x.Id == user_id)
        .Update(x => new Staff
        {
          // LastIp = ip_address(),
          LastLogin = DateTime.Now
        });
      db.SaveChanges();
      log_activity($"User Successfully Logged In [User Id: {user_id}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
      return;
    }

    db.Contacts
      .Where(x => x.Id == user_id)
      .Update(x => new Contact
      {
        // LastIp = ip_address(),
        LastLogin = DateTime.Now
      });
    log_activity($"User Successfully Logged In [User Id: {user_id}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
  }

  /**
   * Send set password email for contacts
   * @param string email
   */
  public bool set_password_email(string email)
  {
    var user = db.Contacts.FirstOrDefault(x => x.Email == email);

    if (user == null) return false;

    if (!user.Active)
      return true;
    // return new { memberinactive = true };

    var new_pass_key = Guid.NewGuid().ToString();
    var items = db.Contacts.Where(x => x.Id == user.Id)
      .ToList()
      .Select(x =>
      {
        x.NewPassKey = new_pass_key;
        x.NewPassKeyRequested = today();
        return x;
      })
      .ToList();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    var contact = db.Contacts.FirstOrDefault(x => x.Id == user.UserId);
    if (contact == null) return false;
    contact.NewPassKey = new_pass_key;
    contact.UserId = user.Id;
    contact.Email = user.Email;
    var sent = self.helper.send_mail_template("customer_contact_set_password", user, contact);
    if (sent == null) return false;
    self.hooks.do_action("set_password_email_sent", new { is_staff_member = false, user });
    return true;
  }

  /**
   * @param  string Email from the user
   * @param  Is Client or Staff
   * @return boolean
   * Generate new password key for the user to reset the password.
   */
  public object forgot_password(string email, bool is_staff = false)
  {
    var jsonString = is_staff
      ? JsonConvert.SerializeObject(db.Staff.FirstOrDefault(x => x.Email == email))
      : JsonConvert.SerializeObject(db.Contacts.FirstOrDefault(x => x.Email == email));


    var user = JsonConvert.DeserializeObject<Contact>(jsonString);


    if (user != null)
    {
      if (user.Active)
      {
        log_activity($"Inactive User Tried Password Reset [Email: {email}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
        return new
        {
          memberinactive = true
        };
      }

      var new_pass_key = Guid.NewGuid().ToString();

      db.Contacts
        .Where(x => x.Id == user.Id)
        .Update(x => new Contact
        {
          NewPassKey = new_pass_key,
          NewPassKeyRequested = today()
        });
      var affected_rows = db.SaveChanges();
      if (affected_rows <= 0) return false;
      var user_data = CurrentUser;
      // data.NewPassKey = new_pass_key;
      // data.IsStaff = is_staff;
      // data.UserId = user.Id;
      var merge_fields = new List<object>();
      var sent = is_staff == false
        ? self.helper.send_mail_template("customer_contact_forgot_password", user.Email, user.UserId, user.Id)
        : self.helper.send_mail_template("staff_forgot_password", user.Email, user.Id);
      if (sent == null) return false;
      log_activity($"Password Reset Email sent [Email: {email}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
      self.hooks.do_action("forgot_password_email_sent", new { is_staff_member = is_staff, user });
      return true;
    }

    log_activity($"Non Existing User Tried Password Reset [Email: {email}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
    return false;
  }

  /**
   * Update user password from forgot password feature or set password
   * @param boolean staff        is staff or contact
   * @param mixed user_id
   * @param string new_pass_key the password generate key
   * @param string password     new password
   */
  public object set_password(bool is_staff, int user_id, string new_pass_key, string password)
  {
    if (!can_set_password(is_staff, user_id, new_pass_key))
      return new
      {
        expired = true
      };

    password = self.HashPassword(password);
    var jsonString = JsonConvert.SerializeObject(is_staff
      ? db.Staff.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key)
      : db.Contacts.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key)
    );

    if (is_staff)
    {
      var staff = JsonConvert.DeserializeObject<Staff>(jsonString);
      if (staff != null)
      {
        staff.Password = password;
        db.Staff.Update(staff);
      }
    }
    else
    {
      var contact = JsonConvert.DeserializeObject<Contact>(jsonString);
      if (contact != null)
      {
        contact.Password = password;
        db.Contacts.Update(contact);
      }
    }

    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity($"User Set Password [User ID: {user_id}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");
    if (is_staff)
    {
      var staff = db.Staff.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (staff != null)
      {
        staff.Password = password;
        staff.NewPassKey = null;
        staff.NewPassKeyRequested = null;
        staff.LastPasswordChange = DateTime.Now;
        db.Staff.Update(staff);
      }
    }
    else
    {
      var contact = db.Contacts.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (contact != null)
      {
        contact.Password = password;
        contact.Password = password;
        contact.NewPassKey = null;
        contact.NewPassKeyRequested = null;
        contact.LastPasswordChange = DateTime.Now;
        db.Contacts.Update(contact);
      }
    }

    db.SaveChanges();

    return true;
  }

  /**
   * @param  boolean Is Client or Staff
   * @param  integer ID
   * @param  string
   * @param  string
   * @return boolean
   * User reset password after successful validation of the key
   */
  public object reset_password(bool is_staff, int user_id, string new_pass_key, string password)
  {
    if (!can_reset_password(is_staff, user_id, new_pass_key)) return new { expired = true };
    password = self.HashPassword(password);
    if (is_staff)
      db.Staff.Where(x => x.Id == user_id && x.NewPassKey == new_pass_key)
        .Update(x => new Staff
        {
          Password = password
        });
    else
      db.Contacts.Where(x => x.Id == user_id && x.NewPassKey == new_pass_key)
        .Update(x => new Contact
        {
          Password = password
        });

    var affected_rows = db.SaveChanges();

    if (affected_rows <= 0) return null;
    log_activity($"User Reseted Password [User ID: {user_id}, Is Staff Member: {(is_staff ? "Yes" : "No")}, IP: {self.input.ip_address()}]");

    if (is_staff)
    {
      var staff = db.Staff.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (staff == null) return false;
      staff.NewPassKey = null;
      staff.NewPassKeyRequested = null;
      staff.LastPasswordChange = DateTime.Now;
      GoUpdate(staff);
      var sent = self.helper.send_mail_template("staff_password_resetted", staff.Email, staff.Id);
      if (sent != null) return true;
    }
    else
    {
      var contact = db.Contacts.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (contact == null) return false;
      contact.NewPassKey = null;
      contact.NewPassKeyRequested = null;
      contact.LastPasswordChange = DateTime.Now;
      GoUpdate(contact);
      var sent = self.helper.send_mail_template("customer_contact_password_resetted", contact.Email, contact.UserId, contact.Id);
      if (sent != null) return true;
    }

    return false;
  }

  /**
   * @param  integer Is Client or Staff
   * @param  integer ID
   * @param  string Password reset key
   * @return boolean
   * Check if the key is not expired or not exists in database
   */
  public bool can_reset_password(bool is_staff, int user_id, string new_pass_key)
  {
    var timestamp_now_minus_1_hour = DateTime.Now.AddHours(-1);
    var new_pass_key_requested = DateTime.Now;
    if (is_staff)
    {
      var staff = db.Staff.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (staff == null) return false;
      new_pass_key_requested = DateTime.Parse(staff.NewPassKeyRequested);
    }
    else
    {
      var contact = db.Contacts.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (contact == null) return false;
      new_pass_key_requested = DateTime.Parse(contact.NewPassKeyRequested);
    }

    return timestamp_now_minus_1_hour <= new_pass_key_requested;
  }

  /**
   * @param  integer Is Client or Staff
   * @param  integer ID
   * @param  string Password reset key
   * @return boolean
   * Check if the key is not expired or not exists in database
   */
  public bool can_set_password(bool staff, int user_id, string new_pass_key)
  {
    var timestampNowMinus48Hour = DateTime.Now.AddHours(-48);
    if (staff)
    {
      var staffs = db.Staff.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
      if (staffs == null) return false;
      return timestampNowMinus48Hour <= DateTime.Parse($"{staffs.NewPassKeyRequested}");
    }

    var contacts = db.Contacts.FirstOrDefault(x => x.Id == user_id && x.NewPassKey == new_pass_key);
    if (contacts == null) return false;
    return timestampNowMinus48Hour <= DateTime.Parse($"{contacts.NewPassKeyRequested}");
  }

  /**
   * Get user from database by 2 factor authentication code
   * @param  string code authentication code to search for
   * @return object
   */
  public Staff? get_user_by_two_factor_auth_code(string code)
  {
    return db.Staff.FirstOrDefault(x => x.TwoFactorAuthCode == code);
  }


  /**
   * Login user via two factor authentication
   * @param  object user user object
   * @return boolean
   */
  public bool two_factor_auth_login(Staff staff)
  {
    var jsonString = JsonConvert.SerializeObject(staff);
    var contact = JsonConvert.DeserializeObject<Contact>(jsonString);
    return contact != null && two_factor_auth_login(contact);
  }

  public bool two_factor_auth_login(Contact user)
  {
    self.hooks.do_action("before_staff_login", new
    {
      email = user.Email,
      user_id = user.Id
    });

    self.input.session.set_userdata(new
    {
      staff_user_id = user.Id,
      staff_logged_in = true
    });

    var remember = false;
    if (self.input.session.has_userdata("tfa_remember"))
    {
      remember = true;
      self.input.session.unset_userdata("tfa_remember");
    }

    if (remember) create_autologin(user.Id, true);
    update_login_info(user.Id, true);
    return true;
  }

  /**
   * Check if 2 factor authentication code sent to email is valid for usage
   * @param  string  code auth code
   * @param  string  email email of staff login in
   * @return boolean
   */
  public bool is_two_factor_code_valid(string code, string email)
  {
    var user = db.Staff.FirstOrDefault(x => x.Email == email && x.TwoFactorAuthCode == code);
    // Code not exists because no user is found
    if (user == null) return false;
    var timestamp_minus_1_hour = DateTime.UtcNow.AddHours(-1);
    var new_code_key_requested = user.TwoFactorAuthCodeRequested;
    // The code is older then 1 hour and its not valid
    return !(timestamp_minus_1_hour > new_code_key_requested);
    // Code is valid
  }

  /**
   * Clears 2 factor authentication code in database
   * @param  mixed id
   * @return boolean
   */
  public bool clear_two_factor_auth_code(int id)
  {
    db.Staff.Where(x => x.Id == id).Update(x => new Staff { TwoFactorAuthCode = null });
    db.SaveChanges();
    return true;
  }

  /**
   * Set 2 factor authentication code for staff member
   * @param mixed id staff id
   */
  public object set_two_factor_auth_code(int id)
  {
    var code = self.helper.generate_two_factor_auth_key();
    code += id;
    db.Staff.Where(x => x.Id == id).Update(x => new Staff
    {
      TwoFactorAuthCode = code,
      TwoFactorAuthCodeRequested = DateTime.Now
    });
    db.SaveChanges();
    return code;
  }

  public void get_qr(string system_name)
  {
    // var staff = get_staff(get_staff_user_id());
    // var g = new GoogleAuthenticator();
    // var secret = g.generateSecret();
    // var username = urlencode(staff.email);
    // var url = \Sonata\GoogleAuthenticator\GoogleQrUrl::generate(username, secret, system_name);
    // return new { qrURL = url, secret };
  }

  public bool set_google_two_factor(string secret)
  {
    var id = staff_user_id;
    secret = encrypt(secret);
    db.Staff.Where(x => x.Id == id).Update(x => new Staff
    {
      TwoFactorAuthEnabled = 2,
      GoogleAuthSecret = secret
    });
    var success = db.SaveChanges();
    return success > 0;
  }

  public bool is_google_two_factor_code_valid(string code, bool? secret = null)
  {
    // var g = new GoogleAuthenticator();
    // if (!secret.HasValue) return g.checkCode(secret, code);
    // var staffid = this.session.userdata("tfa_staffid");
    // var staff = db.Staff.FirstOrDefault(x => x.Id == staffid);
    // if (staff != null && !string.IsNullOrEmpty(staff.google_auth_secret))
    //   return g.checkCode(
    //     decrypt(staff.GoogleAuthSecret),
    //     code
    //   );

    return false;
  }

  public string encrypt(string str)
  {
    // return this.encryption.encrypt(str);
    return string.Empty;
  }

  public string decrypt(string str)
  {
    // return this.encryption.decrypt(str);
    return string.Empty;
  }
}
