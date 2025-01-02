using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Helpers.Tags;
using Service.Schemas.Ui.Entities;

namespace Service.Helpers;

using Models.Contracts;

public static class ClientHelper
{
  /**
 * Check if staff member have assigned customers
 * @param  mixed staff_id staff id
 * @return boolean
 */
  public static bool have_assigned_customers(this MyContext db, int? staff_id = null)
  {
    staff_id ??= db.get_staff_user_id();
    var cache = Convert.ToInt32(app_object_cache.get<string>($"staff-total-assigned-customers-{staff_id}"));
    var result = 0;
    if (cache != null)
      result = cache;
    else
      // result = fapp.CustomerAdmins.Count(x => x.StaffId == staff_id);
      app_object_cache.add($"staff-total-assigned-customers-{staff_id}", result);

    return result > 0;
  }

  /**
 * Get primary contact user id for specific customer
 * @param  mixed $userid
 * @return mixed
 */
  public static int get_primary_contact_user_id(this HelperBase helper, int? userid)
  {
    if (!userid.HasValue) return 0;
    var db = new MyContext();
    var row = db.Contacts.FirstOrDefault(x => x.UserId == userid && x.IsPrimary.HasValue && x.IsPrimary.Value);
    return row?.Id ?? 0;
  }

  /**
   * Get client full name
   * @param  string $contact_id Optional
   * @return string Firstname and Lastname
   */
  public static string get_contact_full_name(this MyContext db)
  {
    return db.get_contact_full_name(db.get_contact_user_id());
  }

  public static string get_contact_full_name(this MyContext db, int contact_id)
  {
    var contact = app_object_cache.get<Contact>("contact-full-name-data-" + contact_id);
    if (contact != null) return contact != null ? $"{contact.FirstName} {contact.LastName}" : "";
    contact = db.Contacts.FirstOrDefault(x => x.Id == contact_id);
    app_object_cache.add("contact-full-name-data-" + contact_id, contact);
    return contact != null ? $"{contact.FirstName} {contact.LastName}" : "";
  }

  /**
     * Predefined contact permission
     * @return array
     */
  public static List<ContactPermissionOption> get_contact_permissions(this HelperBase helper)
  {
    var permissions = new List<ContactPermissionOption>
    {
      new()
      {
        Id = 1,
        Name = helper.label("customer_permission_invoice"),
        ShortName = "invoices"
      },
      new()
      {
        Id = 2,
        Name = helper.label("customer_permission_estimate"),
        ShortName = "estimates"
      },
      new()
      {
        Id = 3,
        Name = helper.label("customer_permission_contract"),
        ShortName = "contracts"
      },
      new()
      {
        Id = 4,
        Name = helper.label("customer_permission_proposal"),
        ShortName = "proposals"
      },
      new()
      {
        Id = 5,
        Name = helper.label("customer_permission_support"),
        ShortName = "support"
      },
      new()
      {
        Id = 6,
        Name = helper.label("customer_permission_projects"),
        ShortName = "projects"
      }
    };

    permissions = hooks.apply_filters("get_contact_permissions", permissions);
    return permissions;
  }

  /**
 * Check whether the user disabled verification emails for contacts
 * @return boolean
 */
  public static bool is_email_verification_enabled(this MyContext db)
  {
    var exist = db.EmailTemplates.Any(x => x.Slug == "contact-verification-email" && x.Active == 0);
    return exist;
  }

  /**
 * Used in:
 * Search contact tickets
 * Project dropdown quick switch
 * Calendar tooltips
 * @param  [type] $userid [description]
 * @return [type]         [description]
 */
  public static string get_company_name(this MyContext db, int? userid = null, bool prevent_empty_company = false)
  {
    var _userid = db.get_client_user_id();
    if (userid.HasValue) _userid = userid.Value;
    // var select = (prevent_empty_company == false ? get_sql_select_client_company() : "company");
    var client = db.Clients.FirstOrDefault(x => x.Id == _userid);
    return client != null ? client.Company : string.Empty;
  }

  /**
 * Get predefined tabs array, used in customer profile
 * @return array
 */
  public static List<Tab> get_customer_profile_tabs(this HelperBase helper)
  {
    // return get_instance()->app_tabs->get_customer_profile_tabs();
    return new List<Tab>();
  }

  public static int? get_user_id_by_contact_id(this MyContext db, int id)
  {
    // Simulate dependency injection or retrieve the necessary services (e.g., caching and database)
    var cacheKey = $"user-id-by-contact-id-{id}";
    var cachedUserId = Convert.ToInt32(_cacheService.get<string>(cacheKey));
    if (cachedUserId <= 0) return cachedUserId;
    var client = db.Contacts
      .Where(c => c.Id == id)
      .Select(c => c.UserId)
      .FirstOrDefault();
    if (client <= 0) return cachedUserId;
    cachedUserId = client;
    _cacheService.add(cacheKey, cachedUserId);
    return cachedUserId;
  }

  /**
 * Function is customer admin
 * @param  mixed  $id       customer id
 * @param  staff_id  $staff_id staff id to check
 * @return boolean
 */
  public static bool is_customer_admin(this MyContext db, int id, int staff_id = 0)
  {
    staff_id = staff_id > 0 ? staff_id : db.get_staff_user_id();
    var cache = app_object_cache.get<Dictionary<string, object>>($"{id}-is-customer-admin-{staff_id}");
    if (cache.Keys.Any())
      return cache.ContainsKey("retval");
    var retval = db.CustomerAdmins.Any(x => x.CustomerId == id && x.StaffId == staff_id);
    app_object_cache.add($"{id}-is-customer-admin-{staff_id}", new { retval });
    return retval;
  }

  /**
 * Return contact profile image url
 * @param  mixed $contact_id
 * @param  string $type
 * @return string
 */
  public static string contact_profile_image_url(this MyContext db, int contact_id, string type = "small")
  {
    var url = base_url("assets/images/user-placeholder.jpg");
    var app_object_cache = new AppObjectCache();
    var path = app_object_cache.get<string>("contact-profile-image-path-" + contact_id);

    if (!string.IsNullOrEmpty(path))
    {
      app_object_cache.add("contact-profile-image-path-" + contact_id, url);
      var contact = db.Contacts.FirstOrDefault(x => x.Id == contact_id);
      if (contact != null && !string.IsNullOrEmpty(contact.ProfileImage))
      {
        path = "uploads/client_profile_images/" + contact_id + '/' + type + '_' + contact.ProfileImage;
        app_object_cache.add("contact-profile-image-path-" + contact_id, path);
      }
    }

    if (!string.IsNullOrEmpty(path) && file_exists(path)) url = base_url(path);
    return url;
  }

  /**
 * Check whether the contact email is verified
 * @since  2.2.0
 * @param  mixed  $id contact id
 * @return boolean
 */
  public static bool is_contact_email_verified(this MyContext db, int? id = null)
  {
    id ??= db.get_contact_user_id();
    var contact = new Contact();
    if (self.globals<Contact>("contact") != null && self.globals<Contact>("contact").Id == id.Value)
    {
      contact = self.globals<Contact>("contact");
      return !string.IsNullOrEmpty(contact.EmailVerifiedAt);
    }

    contact = db.Contacts.FirstOrDefault(x => x.Id == id);
    if (contact == null) return false;
    return !string.IsNullOrEmpty(contact.EmailVerifiedAt);
  }

  public static void send_customer_registered_email_to_administrators(this HelperBase helper, int client_id)
  {
    var staff_model = self.staff_model(db);
    var admins = staff_model.get(x => x.Active == true && x.IsAdmin == true);
    admins.ForEach(admin => { self.helper.send_mail_template("customer_new_registration_to_admins", admin.Email, client_id, admin.Id); });
  }

  public static bool client_logged_in(this MyContext db)
  {
    return db.is_client_logged_in();
  }
}
