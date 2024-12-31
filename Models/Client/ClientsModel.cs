using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Framework.Schemas;
using Service.Helpers;
using Service.Libraries.MergeField;
using Service.Models.Contracts;
using Service.Models.CreditNotes;
using Service.Models.Estimates;
using Service.Models.Invoices;
using Service.Models.Statements;
using Service.Models.Tasks;
using Service.Models.Projects;
using Service.Models.Proposals;
using Service.Models.Tickets;
using File = Service.Entities.File;

namespace Service.Models.Client;

public class ClientsModel(MyInstance self, MyContext db) : MyModel(self)
{
  private List<string> contact_columns => self.hooks.apply_filters("contact_columns", new List<string> { "firstname", "lastname", "email", "phonenumber", "title", "password", "send_set_password_email", "donotsendwelcomeemail", "permissions", "direction", "invoice_emails", "estimate_emails", "credit_note_emails", "contract_emails", "task_emails", "project_emails", "ticket_emails", "is_primary" });

  private AuthenticationModel authentication_model = self.model.authentication_model();
  private ClientVaultEntriesModel client_vault_entries_model = self.model.client_vault_entries_model();
  private ClientGroupsModel client_groups_model = self.model.client_groups_model();
  private StatementModel statement_model = self.model.statement_model();
  private TicketsModel tickets_model = self.model.tickets_model();
  private InvoicesModel invoices_model = self.model.invoices_model();
  private CreditNotesModel credit_notes_model = self.model.credit_notes_model();
  private TasksModel tasks_model = self.model.tasks_model();
  private RolesModel roles_model = self.model.roles_model();
  private EstimatesModel estimates_model = self.model.estimates_model();
  private SubscriptionsModel subscriptions_model = self.model.subscriptions_model();
  private ContractsModel contracts_model = self.model.contracts_model();
  private ProjectsModel projects_model = self.model.projects_model();
  private ProposalsModel proposals_model = self.model.proposals_model();
  private ExpensesModel expenses_model = self.model.expenses_model();

  /**
   * Get client object based on passed clientid if not passed clientid return array of all clients
   * @param  mixed id    client id
   * @param  array  where
   * @return mixed
   */
  public Entities.Client? get(int id, Expression<Func<Entities.Client, bool>> condition)
  {
    var query = db.Clients
      .Include(x => x.Country)
      .Where(condition)
      // .Include(x => x.Contacts)
      .Where(x => x.Id == id);

    query = query.Where(x => x.Id == id);
    var client = query.FirstOrDefault();
    if (client != null && db.get_option_compare("company_requires_vat_number_field", 0)) client.Vat = null;
    // GLOBALS['client'] = client;
    return client;
  }

  public List<Entities.Client> get(Expression<Func<Entities.Client, bool>> where)
  {
    var query = db.Clients
      .Include(x => x.Country)
      // .Include(x => x.Contacts)
      .Where(where)
      .OrderBy(x => x.Company)
      .ToList();
    return query;
  }

  /**
   * Get customers contacts
   * @param  mixed customer_id
   * @param  array where       perform where query
   * @param  array whereIn     perform whereIn query
   * @return array
   */
  public List<Contact> get_contacts()
  {
    var condition = CreateCondition<Contact>(x => x.Active);
    var whereIn = CreateCondition<Contact>(x => x.Active);
    return get_contacts(condition, whereIn);
  }

  public List<Contact> get_contacts(int customer_id = 0)
  {
    var query = db.Contacts.AsQueryable();
    query = query.Where(x => x.UserId == customer_id);

    // foreach (whereIn as key => values)
    // {
    //   if (is_string(key) && is_array(values))
    //   {
    //     this.db.where_in(key, values);
    //   }
    // }
    query = query.OrderByDescending(x => x.IsPrimary);

    return query.ToList();
  }
  // public  void get_contacts(int customer_id = 0, where = ['active' => 1], whereIn = [])
  // {

  // }

  public List<Contact> get_contacts(Expression<Func<Contact, bool>> condition, Expression<Func<Contact, bool>> whereIn)
  {
    var query = db.Contacts.Where(condition).AsQueryable();
    // if (customer_id.HasValue) query.Where(x => x.UserId == customer_id);
    if (whereIn != null) query.Where(whereIn);
    return query
      .OrderByDescending(x => x.IsPrimary)
      .ToList();
  }

  /**
   * Get single contacts
   * @param  mixed id contact id
   * @return object
   */
  public Contact? get_contact(int id)
  {
    return db.Contacts.FirstOrDefault(x => x.Id == id);
  }

  /**
   * Get contact by given email
   *
   * @since 2.8.0
   *
   * @param  string email
   *
   * @return \strClass|null
   */
  public Contact? get_contact_by_email(string email)
  {
    return db.Contacts.FirstOrDefault(x => x.Email == email);
  }

  /**
   * @param array _POST data
   * @param withContact
   *
   * @return integer Insert ID
   *
   * Add new client to database
   */
  public int add(Entities.Client data, int? withContact = null)
  {
    var client_id = 0;
    var contact_data = new Contact();
    // From Lead Convert to client
    // if ( (data['send_set_password_email'])) contact_data['send_set_password_email'] = true;
    // if ( (do_not_send_welcome_email)) contact_do_not_send_welcome_email = true;

    data = check_zero_columns(data);

    data = self.hooks.apply_filters("before_client_added", data);

    // foreach (var field in contact_columns.Where(field => data[field]))
    //   contact_data[field] = data[field];
    // Phonenumber is also used for the company profile
    // if (field != "phonenumber")
    //   unset(data[field]);
    var groups_in = new List<int>();
    // groups_in = Arr::pull(data, 'groups_in') ?? [];
    // custom_fields = Arr::pull(data, 'custom_fields') ?? [];
    var custom_fields = new List<CustomField>();

    // From customer profile register
    if (string.IsNullOrEmpty(data.PhoneNumber)) contact_data.PhoneNumber = data.PhoneNumber;


    data.DateCreated = DateTime.Now;
    data.AddedFrom = staff_user_id;
    var result = db.Clients.Add(data);

    if (result.IsAdded()) return result.Entity.Id;
    if (custom_fields.Any())
    {
      var _custom_fields = custom_fields;
      // Possible request from the register area with 2 types of custom fields for contact and for comapny/customer

      switch (custom_fields.Count())
      {
        case 1:
        {
          var row = _custom_fields.First();
          // if (row.Contacts)
          // {
          //   contact_data.CustomField.Contacts = _custom_fields.Contacts;
          //   unset(custom_fields);
          // }

          break;
        }
        case 2:
          custom_fields.Clear();
          // custom_fields.Customer = _custom_fields.Customers;
          // contact_data.CustomField.Contacts = _custom_fields.Contacts;
          break;
      }

      self.helper.handle_custom_fields_post(client_id, custom_fields);
    }

    var contact_id = withContact ?? add_contact(contact_data, client_id, not_manual_request: withContact.Value);

    foreach (var group in groups_in)
      db.CustomerGroups.Add(new CustomerGroup
      {
        CustomerId = client_id,
        GroupId = group
      });

    var log = $"ID: {client_id}";

    if (log == "" && contact_id == 0) log = self.helper.get_contact_full_name(contact_id);

    var isStaff = 0;

    if (!client_logged_in && self.helper.is_staff_logged_in())
    {
      log += $", From Staff: {staff_user_id}";
      isStaff = staff_user_id;
    }

    // //do_action_deprecated("after_client_added", new[] { client_id }, "2.9.4", "after_client_created");

    self.hooks.do_action("after_client_created", new
    {
      id = client_id,
      data,
      contact_data,
      custom_fields,
      groups_in,
      withContact
    });

    log_activity($"New Client Created [{log}],IsStaff : {isStaff}");
    return client_id;
  }

  /**
   * @param  array _POST data
   * @param  integer ID
   * @return boolean
   * Update client informations
   */
  public bool update(Entities.Client data, int id, bool client_request = false)
  {
    var updated = false;
    data = check_zero_columns(data);
    data = self.hooks.apply_filters("before_client_updated", data, id);

    var update_all_other_transactions = false;
    // update_all_other_transactions = (bool)Arr::pull(data, 'update_all_other_transactions');
    var update_credit_notes = false;
    // update_credit_notes = (bool)Arr::pull(data, 'update_credit_notes');
    // custom_fields = Arr::pull(data, 'custom_fields') ?? [];
    var custom_fields = new List<CustomField>();
    // groups_in = Arr::pull(data, 'groups_in') ?? false;
    var groups_in = new List<int>();

    if (self.helper.handle_custom_fields_post(id, custom_fields)) updated = true;
    var result = db.Clients.Where(x => x.Id == id).Update(x => data);

    if (result > 0) updated = true;

    if (update_all_other_transactions || update_credit_notes)
    {
      var transactions_update = new Invoice
      {
        BillingStreet = data.BillingStreet,
        BillingCity = data.BillingCity,
        BillingState = data.BillingState,
        BillingZip = data.BillingZip,
        BillingCountry = data.BillingCountry,
        ShippingStreet = data.ShippingStreet,
        ShippingCity = data.ShippingCity,
        ShippingState = data.ShippingState,
        ShippingZip = data.ShippingZip,
        ShippingCountry = data.ShippingCountry
      };

      if (update_all_other_transactions)
      {
        // Update all invoices except paid ones.
        var affected_rows = db.Invoices.Where(x => x.ClientId == id && x.Status != 2).Update(x => transactions_update);

        if (affected_rows > 0) updated = true;

        // Update all estimates

        affected_rows = db.Estimates.Where(x => x.ClientId == id).Update(x => transactions_update);
        if (affected_rows > 0) updated = true;
      }

      if (update_credit_notes)
      {
        var affected_rows = db.CreditNotes.Where(x => x.ClientId == id && x.Status != 2).Update(x => transactions_update);

        if (affected_rows > 0) updated = true;
      }
    }

    if (client_groups_model.sync_customer_groups(id, groups_in.ToArray())) updated = true;
    //do_action_deprecated("after_client_updated", new[] { id }, "2.9.4", "client_updated");

    self.hooks.do_action("client_updated", new
    {
      id,
      data,
      update_all_other_transactions,
      update_credit_notes,
      custom_fields,
      groups_in,
      updated
    });

    if (updated) log_activity($"Customer Info Updated [ID: {id}]");
    return updated;
  }

  /**
   * Update contact data
   * @param  array  data           _POST data
   * @param  mixed  id             contact id
   * @param  boolean client_request is request from customers area
   * @return mixed
   */
  public object update_contact(Contact data, int id, bool client_request = false, bool? send_set_password_email = null)
  {
    var affectedRows = 0;
    var contact = get_contact(id);
    if (string.IsNullOrEmpty(data.Password))
    {
      data.Password = null;
    }
    else
    {
      data.Password = self.HashPassword(data.Password);
      data.LastPasswordChange = DateTime.Now;
    }

    send_set_password_email = send_set_password_email.HasValue ? true : false;
    var set_password_email_sent = false;
    // var permissions = data.Permissions ? data.Permissions : new List<int>();
    var permissions = new List<int>();
    // data.IsPrimary = data.IsPrimary ? 1 : 0;

    // Contact cant change if is primary or not
    if (client_request)
      data.IsPrimary = false;

    // if (data.CustomField)
    // {
    //   var custom_fields = data.CustomField;
    //   if (handle_custom_fields_post(id, custom_fields)) affectedRows++;
    //   (data.CustomField);
    // }

    // if (client_request == false)
    // {
    //   data.InvoiceEmails = data.InvoiceEmails ? 1 : 0;
    //   data.EstimateEmails = data.EstimateEmails ? 1 : 0;
    //   data.CreditNoteEmails = data.CreditNoteEmails ? 1 : 0;
    //   data.ContractEmails = data.ContractEmails ? 1 : 0;
    //   data.TaskEmails = data.TaskEmails ? 1 : 0;
    //   data.ProjectEmails = data.ProjectEmails ? 1 : 0;
    //   data.TicketEmails = data.TicketEmails ? 1 : 0;
    // }

    // data = self.hooks.apply_filters("before_update_contact", data, id);
    data = self.hooks.apply_filters("before_update_contact", data);


    var affected_rows = db.Contacts
      .Where(x => x.Id == id)
      .Update(x => data);
    if (affected_rows > 0)
    {
      affectedRows++;
      if (data.IsPrimary!.Value)
        db.Contacts
          .Where(x => x.UserId == contact.UserId && x.Id != id)
          .Update(x => new Contact { IsPrimary = false });
    }

    if (client_request == false)
    {
      var customer_permissions = roles_model.get_contact_permissions(id);
      if (customer_permissions.Any())
      {
        customer_permissions.Where(x => permissions.Contains(x.PermissionId))
          .ToList()
          .ForEach(x =>
          {
            db.ContactPermissions.Where(y =>
              x.UserId == id &&
              x.PermissionId == y.PermissionId
            ).Delete();
            permissions.Remove(x.PermissionId);
          });

        permissions.Select(permission =>
          {
            var _exists = db.ContactPermissions.Any(x => x.UserId == id && x.PermissionId == permission);
            if (_exists) return 0;
            var result = db.ContactPermissions.Add(new ContactPermission
            {
              UserId = id,
              PermissionId = permission
            });
            return result.IsAdded() ? 0 : permission;
          })
          .ToList()
          .Where(x => x > 0)
          .ToList();
      }
      else
      {
        permissions.ForEach(permission =>
        {
          var result = db.ContactPermissions.Add(new ContactPermission
          {
            UserId = id,
            PermissionId = permission
          });
          if (result.IsAdded()) affectedRows++;
        });
      }

      if (send_set_password_email.Value)
        set_password_email_sent = authentication_model.set_password_email(data.Email);
    }

    if (client_request && send_set_password_email.Value)
      set_password_email_sent = authentication_model.set_password_email(data.Email);
    if (affectedRows > 0)
      self.hooks.do_action("contact_updated", id, data);


    switch (affectedRows)
    {
      case > 0 when !set_password_email_sent:
        log_activity($"Contact Updated [ID: {id}]");
        return true;
      case > 0 when set_password_email_sent:
        return new { set_password_email_sent_and_profile_updated = true };
      case 0 when set_password_email_sent:
        return new { set_password_email_sent = true };
      default:
        return false;
    }
  }

  /**
   * Add new contact
   * @param array  data               _POST data
   * @param mixed  customer_id        customer id
   * @param boolean not_manual_request is manual from admin area customer profile or register, convert to lead
   */
  public int add_contact(Contact data, int customer_id, CustomField? custom_fields = null, int? not_manual_request = null, bool send_set_password_email = false, bool? do_not_send_welcome_email = null)
  {
    // if ( (data.Permissions))
    // {
    //   permissions = data.Permissions;
    //   unset(data.Permissions);
    // }

    data.EmailVerifiedAt = today();

    var send_welcome_email = do_not_send_welcome_email is not true;

    if (self.helper.defined("CONTACT_REGISTERING"))
    {
      send_welcome_email = true;

      // Do not send welcome email if confirmation for registration is enabled
      if (db.get_option_compare("customers_register_require_confirmation", 1)) send_welcome_email = false;

      // If client register set this contact as primary
      data.IsPrimary = true;

      if (self.helper.is_email_verification_enabled() && !string.IsNullOrEmpty(data.Email))
      {
        // Verification is required on register
        data.EmailVerifiedAt = null;
        data.EmailVerificationKey = self.helper.uuid();
      }
    }

    if (data.IsPrimary == false)
    {
      data.IsPrimary = true;
      db.Contacts
        .Where(x => x.UserId == customer_id)
        .Update(x => new Contact { IsPrimary = false });
    }
    else
    {
      data.IsPrimary = false;
    }

    var password_before_hash = "";
    data.UserId = customer_id;
    if (!string.IsNullOrEmpty(data.Password))
    {
      password_before_hash = data.Password;
      data.Password = self.HashPassword(data.Password);
    }

    data.DateCreated = DateTime.Now;

    if (!not_manual_request.HasValue)
    {
      data.InvoiceEmails = data.InvoiceEmails > 0 ? 1 : 0;
      data.EstimateEmails = data.EstimateEmails > 0 ? 1 : 0;
      data.CreditNoteEmails = data.CreditNoteEmails > 0 ? 1 : 0;
      data.ContractEmails = data.ContractEmails > 0 ? 1 : 0;
      data.TaskEmails = data.TaskEmails > 0 ? 1 : 0;
      data.ProjectEmails = data.ProjectEmails > 0 ? 1 : 0;
      data.TicketEmails = data.TicketEmails > 0 ? 1 : 0;
    }

    data.Email = data.Email.Trim();

    data = self.hooks.apply_filters("before_create_contact", data);


    var result = db.Contacts.Add(data);
    var contact_id = result.Entity.Id;

    if (!result.IsAdded()) return 0;
    var permissions = new List<int>();
    if (custom_fields != null)
      self.helper.handle_custom_fields_post(contact_id, custom_fields);
    // request from admin area
    if (!permissions.Any() && !not_manual_request.HasValue)
    {
      permissions.Clear();
    }
    else if (not_manual_request.HasValue)
    {
      permissions = new List<int>();
      var _permissions = self.helper.get_contact_permissions();
      var default_permissions = JsonConvert.DeserializeObject<List<ContactPermission>>(db.get_option("default_contact_permissions"));
      if (default_permissions.Any()) permissions = default_permissions.Select(x => x.Id).ToList();
      if (default_permissions.Any()) permissions = _permissions.Select(x => x.Id).ToList();
    }

    if (not_manual_request.HasValue)
      // update all email notifications to 0
      db.Contacts.Where(x => x.Id == contact_id).Update(x => new Contact
      {
        InvoiceEmails = 0,
        EstimateEmails = 0,
        CreditNoteEmails = 0,
        ContractEmails = 0,
        TaskEmails = 0,
        ProjectEmails = 0,
        TicketEmails = 0
      });
    permissions.ForEach(permission =>
    {
      db.ContactPermissions.Add(new ContactPermission
      {
        UserId = contact_id,
        PermissionId = permission
      });
      // Auto set email notifications based on permissions
      if (!not_manual_request.HasValue) return;

      var contactToUpdate = db.Contacts.FirstOrDefault(x => x.Id == contact_id);
      if (contactToUpdate == null) return;
      switch (permission)
      {
        case 1:
          contactToUpdate.InvoiceEmails = 1;
          contactToUpdate.CreditNoteEmails = 1;
          break;
        case 2:
          contactToUpdate.EstimateEmails = 1;
          break;
        case 3:
          contactToUpdate.ContractEmails = 1;
          break;
        case 5:
          contactToUpdate.TicketEmails = 1;
          break;
        case 6:
          contactToUpdate.ProjectEmails = 1;
          contactToUpdate.TaskEmails = 1;
          break;
      }

      db.SaveChanges(); // Save the changes after updating
    });


    if (send_welcome_email && !string.IsNullOrEmpty(data.Email))
      self.helper.send_mail_template(
        "customer_created_welcome_mail",
        data.Email,
        data.UserId,
        contact_id,
        password_before_hash
      );

    if (send_set_password_email) authentication_model.set_password_email(data.Email);

    if (self.helper.defined("CONTACT_REGISTERING"))
      send_verification_email(contact_id);
    else
      // User already verified because is added from admin area, try to transfer any tickets
      tickets_model.transfer_email_tickets_to_contact(data.Email, contact_id);

    log_activity($"Contact Created [ID: {contact_id}]");

    self.hooks.do_action("contact_created", contact_id);

    return contact_id;
  }

  /**
   * Add new contact via customers area
   *
   * @param array  data
   * @param mixed  customer_id
   */
  public int add_contact_via_customers_area(Contact data, int customer_id, List<CustomField> custom_fields = default, bool? do_not_send_welcome_email = null, bool? send_set_password_email = null)
  {
    var send_welcome_email = !do_not_send_welcome_email.HasValue || !do_not_send_welcome_email.Value;
    // var send_set_password_email = do_not_send_welcome_email.HasValue  data['send_set_password_email'] && data['send_set_password_email'] ? true : false;
    send_set_password_email = !send_set_password_email.HasValue || !send_set_password_email.Value;
    // unset(data.CustomField);

    if (!self.helper.is_email_verification_enabled())
    {
      data.EmailVerifiedAt = today();
    }
    else if (self.helper.is_email_verification_enabled() && !string.IsNullOrEmpty(data.Email))
    {
      // Verification is required on register
      data.EmailVerifiedAt = null;
      data.EmailVerificationKey = self.helper.uuid();
    }

    var password_before_hash = data.Password;
    data.DateCreated = DateTime.Now;
    data.UserId = customer_id;
    data.Password = self.HashPassword(string.IsNullOrEmpty(data.Password) ? data.Password : today());


    data = self.hooks.apply_filters("before_create_contact", data);
    var result = db.Contacts.Add(data);


    var contact_id = result.Entity.Id;
    if (contact_id == 0) return 0;
    self.helper.handle_custom_fields_post(contact_id, custom_fields);

    // Apply default permissions
    var default_permissions = JsonConvert.DeserializeObject<List<int>>(db.get_option("default_contact_permissions"));

    self.helper.get_contact_permissions()
      .Where(permission => default_permissions.Contains(permission.Id))
      .ToList()
      .ForEach(permission =>
      {
        db.ContactPermissions.Add(new ContactPermission
        {
          UserId = contact_id,
          PermissionId = permission.Id
        });
      });

    if (send_welcome_email)
      self.helper.send_mail_template(
        "customer_created_welcome_mail",
        data.Email,
        customer_id,
        contact_id,
        password_before_hash
      );

    if (send_set_password_email.HasValue && send_set_password_email.Value)
      authentication_model.set_password_email(data.Email);

    log_activity($"Contact Created [ID: {contact_id}]");
    self.hooks.do_action("contact_created", contact_id);

    return contact_id;
  }

  /**
   * Used to update company details from customers area
   * @param  array data _POST data
   * @param  mixed id
   * @return boolean
   */
  public bool update_company_details(Entities.Client data, int id = 0, List<CustomField> custom_fields = default)
  {
    var affectedRows = 0;
    if (custom_fields.Any())
    {
      if (self.helper.handle_custom_fields_post(id, custom_fields)) affectedRows++;
      custom_fields.Clear();
    }

    data.CountryId ??= 0;
    data.BillingCountry ??= 0;
    data.ShippingCountry ??= 0;
    // From v.1.9.4 these fields are textareas
    data.Address = data.Address.Trim();
    data.Address = data.Address.nl2br();
    if (string.IsNullOrEmpty(data.BillingStreet))
    {
      data.BillingStreet = data.BillingStreet.Trim();
      data.BillingStreet = data.BillingStreet.nl2br();
    }

    if (string.IsNullOrEmpty(data.ShippingStreet))
    {
      data.ShippingStreet = data.ShippingStreet.Trim();
      data.ShippingStreet = data.ShippingStreet.nl2br();
    }

    data = self.hooks.apply_filters("customer_update_company_info", data);
    var result = db.Clients.Where(x => x.Id == id).Update(x => data);
    if (result > 0) affectedRows++;
    if (affectedRows <= 0) return false;
    self.hooks.do_action("customer_updated_company_info", id);
    log_activity($"Customer Info Updated From Clients Area [ID: {id}]");
    return true;
  }

  /**
   * Get customer staff members that are added as customer admins
   * @param  mixed id customer id
   * @return array
   */
  public List<CustomerAdmin> get_admins(int id)
  {
    var rows = db.CustomerAdmins
      .Where(x => x.CustomerId == id)
      .ToList();
    return rows;
  }

  /**
   * Get unique staff id's of customer admins
   * @return array
   */
  public List<int> get_customers_admin_unique_ids()
  {
    var output = db.CustomerAdmins.Select(x => x.StaffId).Distinct().ToList();
    return output;
  }

  /**
   * Assign staff members as admin to customers
   * @param  array data _POST data
   * @param  mixed id   customer id
   * @return boolean
   */
  public bool assign_admins(Staff data, int id)
  {
    var affectedRows = 0;

    if (data != null)
    {
      var affected_rows = db.CustomerAdmins.Where(x => x.CustomerId == id).Delete();
      if (affected_rows > 0) affectedRows++;
    }
    else
    {
      var current_admins = get_admins(id);
      var current_admins_ids = current_admins.Select(x => x.Id).ToList();
      current_admins_ids.ForEach(c_admin_id =>
      {
        // data.CustomerAdmins.Contains(c_admin_id)
        // if (in_array(c_admin_id, data.CustomerAdmins['customer_admins'])) return;
        var affected_rows = db.CustomerAdmins
          .Where(x => x.StaffId == c_admin_id && x.CustomerId == id)
          .Delete();
        if (affected_rows > 0) affectedRows++;
      });


      data.CustomerAdmins
        .ToList()
        .ForEach(n_admin =>
        {
          if (!db.CustomerAdmins.Any(x => x.CustomerId == id && x.StaffId == n_admin.Id)) return;
          var result = db.CustomerAdmins.Add(new CustomerAdmin
          {
            CustomerId = id,
            StaffId = n_admin.Id,
            DateAssigned = today()
          });

          if (result.IsAdded()) affectedRows++;
        });
    }

    return affectedRows > 0;
  }

  /**
   * @param  integer ID
   * @return boolean
   * Delete client, also deleting rows from, dismissed client announcements, ticket replies, tickets, autologin, user notes
   */
  public bool delete(int id)
  {
    var affectedRows = 0;
    // if (!self.helper.is_gdpr() && is_reference_in_table('clientid', 'invoices', id)) return new { referenced = true };
    // if (!self.helper.is_gdpr() && is_reference_in_table('clientid', 'estimates', id)) return new { referenced = true };
    // if (!self.helper.is_gdpr() && is_reference_in_table('clientid', 'creditnotes', id)) return new { referenced = true };

    self.hooks.do_action("before_client_deleted", id);
    var last_activity = self.helper.get_last_system_activity_id();
    var company = self.helper.get_company_name(id);
    db.Clients.RemoveRange(db.Clients.Where(x => x.Id == id));
    var affected_rows = db.SaveChanges();
    if (affected_rows > 0)
    {
      affectedRows++;
      // Delete all user contacts

      db.Contacts
        .Where(x => x.UserId == id).ToList()
        .ForEach(x => delete_contact(x.Id));

      // Delete all tickets start here

      var tickets = db.Tickets.Where(x => x.UserId == id).ToList();


      foreach (var ticket in tickets) tickets_model.delete(ticket.Id);


      db.Notes.Where(x => x.RelId == id && x.RelType == "customer").Delete();

      if (self.helper.is_gdpr() && db.get_option_compare("gdpr_on_forgotten_remove_invoices_credit_notes", 1))
      {
        db.Invoices
          .Where(x => x.ClientId == id)
          .ToList()
          .ForEach(x =>
            invoices_model.delete(x.Id, true)
          );


        db.CreditNotes.Where(x => x.ClientId == id).ToList().ForEach(x =>
          credit_notes_model.delete(x.Id, true)
        );
      }
      else if (self.helper.is_gdpr())
      {
        db.Invoices.Where(x => x.ClientId == id).Delete();
        db.CreditNotes.Where(x => x.ClientId == id).Delete();
      }

      db.CreditNotes.Where(x => x.ClientId == id).Update(x => new CreditNote
      {
        ClientId = 0,
        ProjectId = 0
      });


      db.Invoices
        .Where(x => x.ClientId == id)
        .Update(x => new Invoice
        {
          ClientId = 0,
          Recurring = string.Empty,
          RecurringType = null,
          CustomRecurring = 0,
          Cycles = 0,
          LastRecurringDate = null,
          ProjectId = 0,
          SubscriptionId = 0,
          CancelOverdueReminders = 1,
          LastOverdueReminder = null,
          LastDueReminder = null
        });


      if (self.helper.is_gdpr() && db.get_option_compare("gdpr_on_forgotten_remove_estimates", 1))
        db.Estimates.Where(x => x.ClientId == id).ToList().ForEach(x => estimates_model.delete(x.Id, true));
      else if (self.helper.is_gdpr())
        db.Estimates
          .Where(x => x.ClientId == id)
          .Update(x => new Estimate
          {
            ClientId = 0,
            ProjectId = 0,
            IsExpiryNotified = true
          });


      db.Estimates.Where(x => x.ClientId == id)
        .Update(x => new Estimate
        {
          ClientId = 0,
          ProjectId = 0,
          IsExpiryNotified = true
        });


      db.Subscriptions.Where(x => x.ClientId == id).ToList().ForEach(x => subscriptions_model.delete(x.Id, true));

      // Get all client contracts
      db.Contracts.Where(x => x.Client == id).ToList().ForEach(x => contracts_model.delete(x.Id));


      // Delete the custom field values
      db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "customers").Delete();

      // Get customer related tasks
      db.Tasks
        .Where(x => x.RelType == "customer" && x.RelId == id)
        .ToList()
        .ForEach(x => tasks_model.delete_task(x.Id, false));
      db.Reminders
        .Where(x => x.RelType == "customer" && x.RelId == id)
        .Delete();
      db.CustomerAdmins
        .Where(x => x.CustomerId == id)
        .Delete();
      db.Vaults
        .Where(x => x.CustomerId == id)
        .Delete();
      db.CustomerGroups
        .Where(x => x.CustomerId == id)
        .Delete();
      db.Proposals
        .Where(x => x.RelId == id && x.RelType == "customer")
        .ToList()
        .ForEach(x => proposals_model.delete(x.Id));
      db.Files
        .Where(x => x.RelId == id && x.RelType == "customer")
        .ToList()
        .ForEach(x => delete_attachment(x.Id));

      db.Expenses
        .Where(x => x.ClientId == id)
        .ToList()
        .Select(x => x.Id)
        .ToList()
        .ForEach(x => expenses_model.delete(x));

      db.UserMeta.Where(x => x.ContactId == id).Delete();

      db.Leads
        .Where(x => x.ClientId == id)
        .Update(x => new Lead
        {
          ClientId = 0
        });

      // Delete all projects
      db.Projects
        .Where(x => x.ClientId == id)
        .ToList()
        .ForEach(x => projects_model.delete(x.Id));
    }

    if (affectedRows <= 0) return false;

    self.hooks.do_action("after_client_deleted", id);

    // Delete activity log caused by delete customer function
    if (last_activity != null)
      db.ActivityLogs
        .Where(x => x.Id > last_activity.Id)
        .Delete();
    log_activity($"Client Deleted [ID: {id}]");

    return true;
  }

  /**
   * Delete customer contact
   * @param  mixed id contact id
   * @return boolean
   */
  public async Task<bool> delete_contact(int id)
  {
    self.hooks.do_action("before_delete_contact", id);


    var result = db.Contacts.FirstOrDefault(x => x.Id == id);
    var customer_id = result.UserId;

    var last_activity = self.helper.get_last_system_activity_id().Id;
    var affected_rows = await db.Contacts.Where(x => x.Id == id).DeleteAsync();

    if (affected_rows <= 0) return false;

    if (self.helper.is_dir(self.helper.get_upload_path_by_type("contact_profile_images") + id))
      self.helper.delete_dir(self.helper.get_upload_path_by_type("contact_profile_images") + id);

    await db.Consents
      .Where(x => x.ContactId == id)
      .DeleteAsync();
    await db.SharedCustomerFiles
      .Where(x => x.ContactId == id)
      .DeleteAsync();
    await db.DismissedAnnouncements
      .Where(x => x.UserId == id && x.IsStaff == false)
      .DeleteAsync();
    await db.CustomFieldsValues
      .Where(x => x.RelId == id && x.FieldTo == "contacts")
      .DeleteAsync();
    await db.ContactPermissions
      .Where(x => x.UserId == id)
      .DeleteAsync();
    await db.UserAutoLogins
      .Where(x => x.UserId == id && x.IsStaff == false)
      .DeleteAsync();
    db.Tickets
      .Where(x => x.ContactId == id && x.UserId == customer_id)
      .ToList()
      .ForEach(x => tickets_model.delete(x.Id));
    db.Tasks
      .Where(x => x.AddedFrom == id && x.IsAddedFromContact == true)
      .ToList()
      .ForEach(task => tasks_model.delete_task(task.Id, false));
    // Added from contact in customer profile
    db.Files
      .Where(x => x.ContactId == id && x.RelType == "customer")
      .ToList()
      .ForEach(x => delete_attachment(x.Id));

    // Remove contact files uploaded to tasks
    db.Files
      .Where(x => x.ContactId == id && x.RelType == "task")
      .ToList()
      .ForEach(x => tasks_model.remove_task_attachment(x.Id));
    db.TaskComments
      .Where(x => x.ContactId == id)
      .ToList()
      .ForEach(x => tasks_model.remove_comment(x.Id, true));

    db.ProjectFiles
      .Where(x => x.ContactId == id)
      .ToList()
      .ForEach(file => projects_model.remove_file(file.Id, false));
    db.ProjectDiscussions
      .Where(x => x.ContactId == id)
      .ToList()
      .ForEach(x => projects_model.delete_discussion(x.Id, false));
    db.ProjectDiscussionComments
      .Where(x => x.ContactId == id)
      .ToList()
      .ForEach(x => projects_model.delete_discussion_comment(x.Id, false));
    await db.UserMeta.Where(x => x.ContactId == id).DeleteAsync();


    await db.MailQueues
      .Where(x => x.Email == result.Email || x.Bcc.Contains(result.Email) || x.Cc.Contains(result.Email))
      .DeleteAsync();

    if (self.helper.is_gdpr())
    {
      // db.ListEmails.Where(x => x.Email == result.Email).Delete();

      if (!string.IsNullOrEmpty(result.LastIp)) await db.KnowedgeBaseArticleFeedbacks.Where(x => x.Ip == result.LastIp).DeleteAsync();

      await db.TicketsPipeLogs.Where(x => x.Email == result.Email).DeleteAsync();
      await db.TrackedMails.Where(x => x.Email == result.Email).DeleteAsync();
      await db.ProjectActivities.Where(x => x.ContactId == id).DeleteAsync();


      var query = db.SalesActivities
        .Where(x => x.AdditionalData.Contains(result.Email) || x.FullName.Contains(result.FirstName + ' ' + result.LastName))
        .Where(x => !string.IsNullOrEmpty(x.AdditionalData) && x.AdditionalData != null)
        .AsQueryable();

      var contactActivityQuery = false;
      if (!string.IsNullOrEmpty(result.Email))
      {
        query.Where(x => x.AdditionalData.Contains(result.Email));
        contactActivityQuery = true;
      }

      if (!string.IsNullOrEmpty(result.FirstName))
      {
        query.Where(x => x.Description.Contains(result.FirstName));
        contactActivityQuery = true;
      }

      if (!string.IsNullOrEmpty(result.LastName))
      {
        query.Where(x => x.Description.Contains(result.LastName));
        contactActivityQuery = true;
      }

      if (!string.IsNullOrEmpty(result.PhoneNumber))
      {
        query.Where(x => x.Description.Contains(result.PhoneNumber));
        contactActivityQuery = true;
      }

      if (!string.IsNullOrEmpty(result.LastIp))
      {
        query.Where(x => x.AdditionalData.Contains(result.LastIp));
        contactActivityQuery = true;
      }

      if (contactActivityQuery) await query.DeleteAsync();
    }

    // Delete activity log caused by delete contact function
    if (last_activity > 0) await db.ActivityLogs.Where(x => x.Id > last_activity).DeleteAsync();

    self.hooks.do_action("contact_deleted", id, result);

    return true;
  }

  /**
   * Get customer default currency
   * @param  mixed id customer id
   * @return mixed
   */
  public int get_customer_default_currency(int id)
  {
    var result = db.Clients.FirstOrDefault(x => x.Id == id);
    return result?.DefaultCurrency ?? 0;
  }

  /**
   *  Get customer billing details
   * @param   mixed id   customer id
   * @return  array
   */
  public object get_customer_billing_and_shipping_details(int id)
  {
    var rows = db.Clients.Where(x => x.Id == id)
      .Select(x => new
      {
        x.BillingStreet,
        x.BillingCity,
        x.BillingState,
        x.BillingZip,
        x.BillingCountry,
        x.ShippingStreet,
        x.ShippingCity,
        x.ShippingState,
        x.ShippingZip,
        x.ShippingCountry
      })
      .ToList();
    rows = rows
      .Select(x =>
      {
        // x.BillingStreet = clear_textarea_breaks(x.BillingStreet);
        // x.ShippingStreet = clear_textarea_breaks(x.ShippingStreet);
        return x;
      })
      .ToList();


    return rows;
  }

  /**
   * Get customer files uploaded in the customer profile
   * @param  mixed id    customer id
   * @param  array  where perform where
   * @return array
   */
  public List<File> get_customer_files(int id, Expression<Func<File, bool>> where)
  {
    var rows = db.Files
      .Where(where)
      .Where(x =>
        x.RelId == id &&
        x.RelType == "customer"
      )
      .OrderByDescending(x => x.DateCreated)
      .ToList();
    return rows;
  }

  /**
   * Delete customer attachment uploaded from the customer profile
   * @param  mixed id attachment id
   * @return boolean
   */
  public bool delete_attachment(int id)
  {
    var attachment = db.Files.FirstOrDefault(x => x.Id == id);
    var deleted = false;
    if (attachment == null) return deleted;

    if (string.IsNullOrEmpty(attachment.External))
    {
      var relPath = self.helper.get_upload_path_by_type("customer") + attachment.RelId + '/';
      var fullPath = relPath + attachment.FileName;
      self.helper.unlink(fullPath);
      var fname = self.helper.file_name(fullPath);
      var fext = self.helper.file_extension(fullPath);
      var thumbPath = $"{relPath}{fname}_thumb.{fext}";
      if (self.helper.file_exists(thumbPath)) self.helper.unlink(thumbPath);
    }


    var affected_rows = db.Files.Where(x => x.Id == id).Delete();
    if (affected_rows > 0)
    {
      deleted = true;
      db.SharedCustomerFiles.Where(x => x.FileId == id).Delete();
      log_activity($"Customer Attachment Deleted [ID: {attachment.RelId}]");
    }

    if (!self.helper.is_dir(self.helper.get_upload_path_by_type("customer") + attachment.RelId)) return deleted;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = self.helper.list_files(self.helper.get_upload_path_by_type("customer") + attachment.RelId);
    if (other_attachments.Any()) self.helper.delete_dir(self.helper.get_upload_path_by_type("customer") + attachment.RelId);


    return deleted;
  }

  /**
   * @param  integer ID
   * @param  integer Status ID
   * @return boolean
   * Update contact status Active/Inactive
   */
  public bool change_contact_status(int id, bool status)
  {
    // status = self.hooks.apply_filters("change_contact_status", status, id);
    status = self.hooks.apply_filters("change_contact_status", status);

    var result = db.Contacts.Where(x => x.Id == id).Update(x => new Contact { Active = status });
    if (result == 0) return false;
    self.hooks.do_action("contact_status_changed", new Contact
    {
      Id = id
      // Status = status
    });
    log_activity($"Contact Status Changed [ContactID: {id} Status(Active/Inactive): {status}]");
    return true;
  }

  /**
   * @param  integer ID
   * @param  integer Status ID
   * @return boolean
   * Update client status Active/Inactive
   */
  public bool change_client_status(int id, bool status)
  {
    var result = db.Clients.Where(x => x.Id == id).Update(x => new Entities.Client { Active = status });

    if (result <= 0) return false;
    self.hooks.do_action("client_status_changed",
      new Entities.Client
      {
        Id = id
        // Status = status
      }
    );

    log_activity($"Customer Status Changed [ID: {id} Status(Active/Inactive): {status}]");
    return true;
  }

  /**
   * Change contact password, used from client area
   * @param  mixed id          contact id to change password
   * @param  string oldPassword old password to verify
   * @param  string newPassword new password
   * @return boolean
   */
  public bool change_contact_password(int id, string oldPassword, string newPassword)
  {
    // Get current password

    var client = db.Contacts.FirstOrDefault(x => x.Id == id);

    if (!self.VerifyPassword(oldPassword, client.Password))
    {
      Console.WriteLine("return new { old_password_not_match = true };");
      // return new { old_password_not_match = true };
      throw new Exception("old_password_not_match");
    }

    var result = db.Contacts.Where(x => x.Id == id)
      .Update(x => new Contact
      {
        LastPasswordChange = DateTime.Now,
        Password = self.HashPassword(newPassword)
      });


    if (result == 0) return false;
    log_activity($"Contact Password Changed [ContactID: {id}]");

    return true;
  }

  /**
   * Get customer groups where customer belongs
   * @param  mixed id customer id
   * @return array
   */
  public List<CustomerGroup> get_customer_groups(int id)
  {
    return client_groups_model.get_customer_groups(id);
  }

  /**
   * Get all customer groups
   * @param  string id
   * @return mixed
   */
  public List<CustomersGroup> get_groups()
  {
    return client_groups_model.get_groups();
  }

  public CustomersGroup? get_groups(int id)
  {
    return client_groups_model.get_groups(id);
  }

  /**
   * Delete customer groups
   * @param  mixed id group id
   * @return boolean
   */
  public Task<DeletedResult> delete_group(int id)
  {
    return client_groups_model.Delete(id);
  }

  /**
   * Add new customer groups
   * @param array data _POST data
   */
  public bool add_group(object data)
  {
    var (self, db) = getInstance();
    var sender = JsonConvert.DeserializeObject<CustomersGroup>(JsonConvert.SerializeObject(data));
    return client_groups_model.add(sender);
  }

  /**
   * Edit customer group
   * @param  array data _POST data
   * @return boolean
   */
  public bool edit_group(object data)
  {
    var sender = JsonConvert.DeserializeObject<CustomersGroup>(JsonConvert.SerializeObject(data));
    return client_groups_model.edit(sender);
  }

  /**
  * Create new vault entry
  * @param  array data        _POST data
  * @param  mixed customer_id customer id
  * @return boolean
*/
  public bool vault_entry_create(object data, int customer_id)
  {
    var sender = JsonConvert.DeserializeObject<Vault>(JsonConvert.SerializeObject(data));
    return client_vault_entries_model.create(sender, customer_id);
  }

  /**
   * Update vault entry
   * @param  mixed id   vault entry id
   * @param  array data _POST data
   * @return boolean
   */
  public bool vault_entry_update(int id, Vault data)
  {
    return client_vault_entries_model.update(id, data);
  }

  /**
   * Delete vault entry
   * @param  mixed id entry id
   * @return boolean
   */
  public bool vault_entry_delete(int id)
  {
    return client_vault_entries_model.delete(id);
  }

  /**
   * Get customer vault entries
   * @param  mixed customer_id
   * @param  array  where       additional wher
   * @return array
   */
  public List<Vault> get_vault_entries(int customer_id, Expression<Func<Vault, bool>> where)
  {
    return client_vault_entries_model.get_by_customer_id(customer_id, where);
  }

  /**
   * Get single vault entry
   * @param  mixed id vault entry id
   * @return object
   */
  public Vault? get_vault_entry(int id)
  {
    return client_vault_entries_model.get(id);
  }

  /**
  * Get customer statement formatted
  * @param  mixed customer_id customer id
  * @param  string from        date from
  * @param  string to          date to
  * @return array
*/
  public async Task<StatementResult> get_statement(int customer_id, string from, string to)
  {
    return await statement_model.get_statement(customer_id, DateTime.Parse(from), DateTime.Parse(to));
  }

  /**
  * Send customer statement to email
  * @param  mixed customer_id customer id
  * @param  array send_to     array of contact emails to send
  * @param  string from        date from
  * @param  string to          date to
  * @param  string cc          email CC
  * @return boolean
*/
  public async Task<bool> send_statement_to_email(int customer_id, List<int> send_to, string from, string to, string cc = "")
  {
    return await statement_model.send_statement_to_email(customer_id, send_to, from, to, cc);
  }

  /**
   * When customer register, mark the contact and the customer as inactive and set the registration_confirmed field to 0
   * @param  mixed client_id  the customer id
   * @return boolean
   */
  public bool require_confirmation(int client_id)
  {
    var contact_id = self.helper.get_primary_contact_user_id(client_id);
    db.Clients
      .Where(x => x.Id == client_id)
      .Update(x => new Entities.Client
      {
        Active = false,
        RegistrationConfirmed = 0
      });
    db.Contacts
      .Where(x => x.Id == contact_id)
      .Update(x => new Contact
      {
        Active = false
      });

    return true;
  }

  public bool confirm_registration(int client_id)
  {
    var contact_id = self.helper.get_primary_contact_user_id(client_id);

    db.Clients.Where(x => x.Id == client_id).Update(x => new Entities.Client
    {
      Active = true,
      RegistrationConfirmed = 1
    });

    db.Contacts
      .Where(x => x.Id == contact_id)
      .Update(x => new Contact
      {
        Active = true
      });
    var contact = get_contact(contact_id);
    if (contact == null) return false;
    self.helper.send_mail_template("customer_registration_confirmed", contact);
    return true;
  }

  public bool send_verification_email(int id)
  {
    var contact = get_contact(id);
    if (string.IsNullOrEmpty(contact.Email)) return false;
    var success = self.helper.send_mail_template("customer_contact_verification", contact);
    if (success != null)
      db.Contacts.Where(x => x.Id == id).Update(x => new Contact
      {
        EmailVerificationSentAt = today()
      });
    return success != null;
  }

  public bool mark_email_as_verified(int id)
  {
    var affected_rows = db.Contacts
      .Where(x => x.Id == id)
      .Update(x => new Contact
      {
        EmailVerifiedAt = today(),
        EmailVerificationKey = null,
        EmailVerificationSentAt = null
      });

    if (affected_rows <= 0) return false;
    var contact = get_contact(id);

    tickets_model.transfer_email_tickets_to_contact(contact.Email, contact.Id);
    return true;
  }

  public List<Entities.Client> get_clients_distinct_countries()
  {
    var rows = db.Clients
      .Include(x => x.Country)
      // .Select(x => new Client() { x.CountryId, x.Country.ShortName })
      .Distinct()
      .ToList();


    return rows;
  }

  public void send_notification_customer_profile_file_uploaded_to_responsible_staff(int contact_id, int customer_id)
  {
    var staff = get_staff_members_that_can_access_customer(customer_id);
    var merge_fields = self.app_merge_fields().format_feature("client_merge_fields", customer_id, contact_id);
    var notifiedUsers = staff.Select(member =>
      {
        mail_template("customer_profile_uploaded_file_to_staff", member.Email, member.Id)
          .set_merge_fields(merge_fields)
          .send();
        var notify = self.helper.add_notification(new Notification
        {
          ToUserId = member.Id,
          Description = "not_customer_uploaded_file",
          Link = $"clients/client/{customer_id}?group=attachments"
        });
        return notify ? member.Id : 0;
      })
      .ToList();
    self.helper.pusher_trigger_notification(notifiedUsers);
  }

  public List<Staff> get_staff_members_that_can_access_customer(int id)
  {
    var output = db.Staff
      .Where(x =>
        x.IsAdmin ||
        x.Id == db.CustomerAdmins
          .Where(y => y.CustomerId == id)
          .Select(y => y.StaffId)
          .FirstOrDefault() ||
        x.Id == db.StaffPermissions
          .Where(y => y.Feature == "customers" && y.Capability == "view")
          .Select(y => y.StaffId)
          .FirstOrDefault()
      )
      .ToList();
    return output;
  }

  private Entities.Client check_zero_columns(Entities.Client data)
  {
    // if (! (data.ShowPrimaryContact)) string.IsNullOrEmpty(data.ShowPrimaryContact);
    // if (( (data.DefaultCurrency) && string.IsNullOrEmpty(data.DefaultCurrency))  ) data.DefaultCurrency = 0;
    // if (( (data.Country) && string.IsNullOrEmpty(data.Country)) || ! (data.Country)) data.Country = 0;
    // if (( (data.BillingCountry) && string.IsNullOrEmpty(data.BillingCountry)) || ! (data.BillingCountry)) data.BillingCountry = 0;
    // if (( (data.ShippingCountry) && string.IsNullOrEmpty(data.ShippingCountry)) || ! (data.ShippingCountry)) data.ShippingCountry = 0;

    return data;
  }

  public void delete_contact_profile_image(int id)
  {
    self.hooks.do_action("before_remove_contact_profile_image");
    if (self.helper.file_exists(self.helper.get_upload_path_by_type("contact_profile_images") + id))
      self.helper.delete_dir(self.helper.get_upload_path_by_type("contact_profile_images") + id);
    db.Contacts
      .Where(x => x.Id == id)
      .Update(x => new Contact { ProfileImage = null });
  }

  /**
   * @param projectId
   * @param  string  tasks_email
   *
   * @return array[]
   */
  public List<Contact> get_contacts_for_project_notifications(int projectId, string type)
  {
    var project = db.Projects.FirstOrDefault(x => x.Id == projectId);
    if (project == null) return new List<Contact>();
    var checker = new List<int> { 1, 2 };
    if (project.ContactNotification.HasValue && !checker.Contains(project.ContactNotification.Value))
      return new List<Contact>();
    var query = db.Contacts.Where(x => x.UserId == project.ClientId && x.Active && x.ProjectEmails == 1).AsQueryable();
    if (project.ContactNotification != 2) return query.ToList();
    var projectContacts = JsonConvert.DeserializeObject<List<int>>(project.NotifyContacts);
    if (projectContacts == null) return new List<Contact>();
    var rows = query
      .Where(x =>
        projectContacts.Contains(x.Id)
      )
      .ToList();
    return rows;
  }
}
