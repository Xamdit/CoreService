using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Core.InputSet;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Sale;
using Service.Helpers.Tags;
using Service.Models.Client;
using Service.Models.Contracts;
using Service.Models.CreditNotes;
using Service.Models.Estimates;
using Service.Models.Payments;
using Service.Models.Projects;
using Service.Models.Statements;
using Service.Models.Tasks;
using File = Service.Entities.File;
using Task = Service.Entities.Task;
using TaskStatus = Service.Models.Tasks.TaskStatus;


namespace Service.Models.Invoices;

public class InvoicesModel(MyInstance self, MyContext db) : MyModel(self)
{
  private PaymentsModel payments_model = self.model.payments_model();
  private ClientsModel clients_model = self.model.clients_model();
  private ProjectsModel projects_model = self.model.projects_model();
  private CurrenciesModel currencies_model = self.model.currencies_model();
  private ExpensesModel expenses_model = self.model.expenses_model();
  private EstimatesModel estimates_model = self.model.estimates_model();
  private TasksModel tasks_model = self.model.tasks_model();
  private CreditNotesModel credit_notes_model = self.model.credit_notes_model();

  private List<int> statuses = new()
  {
    InvoiceStatus.STATUS_UNPAID,
    InvoiceStatus.STATUS_PAID,
    InvoiceStatus.STATUS_PARTIALLY,
    InvoiceStatus.STATUS_OVERDUE,
    InvoiceStatus.STATUS_CANCELLED,
    InvoiceStatus.STATUS_DRAFT
  };

  private List<string> shipping_fields = new()
  {
    "shipping_street",
    "shipping_city",
    "shipping_city",
    "shipping_state",
    "shipping_zip",
    "shipping_country"
  };

  private EmailScheduleModel email_schedule_model = self.model.email_schedule_model();

  public List<int> get_statuses()
  {
    return statuses;
  }

  public List<Staff?> get_sale_agents()
  {
    var result = db.Invoices
      .Include(x => x.SaleAgentNavigation)
      .Where(x => x.SaleAgent != 0)
      .Select(x => x.SaleAgentNavigation)
      .Distinct()
      .ToList();
    return result;
  }

  public async Task<List<Invoice>> get_unpaid_invoices()
  {
    // Step 1: Check if the user has permission to view all invoices or only their own
    var canViewAllInvoices = self.helper.staff_can(view: "invoices");


    // Step 2: Build the query dynamically
    var invoicesQuery = db.Invoices.AsQueryable();

    if (!canViewAllInvoices)
    {
      // Apply the staff-specific filter based on their permissions
      var staffFilter = await self.helper.get_invoices_where_sql_for_staff(staff_user_id);
      invoicesQuery = invoicesQuery.Where(staffFilter);
    }

    // Step 3: Join with Clients entity and apply additional filters
    invoicesQuery = invoicesQuery
      .Include(i => i.Client) // Assuming a navigation property exists for Client
      .Where(i => i.Status != InvoiceStatus.STATUS_CANCELLED && i.Status != InvoiceStatus.STATUS_PAID)
      .Where(i => i.Total > 0)
      .OrderByDescending(i => i.Number)
      .ThenByDescending(i => DateTime.Parse(i.Date).Year);

    // Step 4: Fetch the invoices
    var invoices = invoicesQuery.ToList();

    // Step 5: Map allowed payment modes and calculate remaining amount for each invoice

    foreach (var invoice in invoices)
    {
      var allowedModes = new List<PaymentMode>();
      var allowedModeIds = JsonConvert.DeserializeObject<List<int>>(invoice.AllowedPaymentModes); // Assuming it's serialized as JSON

      // foreach (var modeId in allowedModeIds) allowedModes.Add( GetPaymentModeById(modeId)); // Assuming this method exists in PaymentModesModel
      // invoice.AllowedPaymentModesList = allowedModes;
      // invoice.TotalLeftToPay = get_invoice_total_left_to_pay(invoice.Id, invoice.Total);
    }

    return invoices;
  }

  /**
   * Get invoice by id
   * @param  mixed  id
   * @return array|object
   */
  public List<Invoice> get()
  {
    return db.Invoices
      .Include(x => x.Currency)
      .ToList();
  }

  public Invoice? get(int id)
  {
    return db.Invoices
      .Include(x => x.Currency)
      .FirstOrDefault(x => x.Id == id);
  }

  public Invoice get(int id, Expression<Func<Invoice, bool>> where)
  {
    var query = db.Invoices.Include(x => x.Currency).AsQueryable();
    query = query.Where(where);
    dynamic invoice = query.FirstOrDefault(x => x.Id == id);
    if (invoice == null) return self.hooks.apply_filters("get_invoice", invoice);
    // invoice.total_left_to_pay = self.helper.get_invoice_total_left_to_pay(invoice.Id, (decimal)invoice.Total);
    invoice.items = self.helper.get_items_by_type("invoice", id);
    invoice.attachments = get_attachments(x => x.Id == id);
    if (invoice.Project != null) invoice.project_data = projects_model.get(invoice.ProjectId);
    invoice.visible_attachments_to_customer_found = false;
    foreach (var attachment in invoice.attachments)
      if (attachment.VisibleToCustomer)
      {
        invoice.visible_attachments_to_customer_found = true;
        break;
      }

    var client = clients_model.get(invoice.client_id);
    invoice.client = client;
    if (!invoice.client)
    {
      invoice.client = new { };
      invoice.client.company = invoice.deleted_customer_name;
    }

    invoice.payments = payments_model.get_invoice_payments(id);
    invoice.scheduled_email = email_schedule_model.get(id, "invoice");
    return self.hooks.apply_filters("get_invoice", invoice);
  }

  // public File? get_attachments(int id)
  // {
  //   // If is passed id get return only 1 attachment
  //   var row = db.Files.FirstOrDefault(x => x.Id == id && x.RelType == "");
  //   return row;
  // }
  //
  // public List<File> get_attachments(int invoice_id, int? id = null)
  // {
  //   // If is passed id get return only 1 attachment
  //   var rows = db.Files.Where(x => x.RelId == invoice_id && x.RelType == "invoice").ToList();
  //   return rows;
  // }

  public List<File> get_attachments(Expression<Func<File, bool>> conditino)
  {
    var rows = db.Files.Where(conditino).ToList();
    rows = rows.Where(x => x.RelType == "invoice").ToList();
    return rows;
  }

  /**
    * @since  2.7.0
    *
    * Check whether the given invoice is draft
    *
    * @param  int   id
    *
    * @return boolean
    */
  private bool is_draft(int id)
  {
    return db.Invoices.Any(x => x.Id == id && x.Status == InvoiceStatus.STATUS_DRAFT);
  }

  /**
     * @since  2.7.0
     *
     * Change the invoice number for status when it's draft
     *
     * @param  int  id
     *
     * @return int
     */
  public int change_invoice_number_when_status_draft(int id)
  {
    var invoice = db.Invoices.FirstOrDefault(x => x.Id == id);
    var newNumber = Convert.ToInt32(db.get_option("next_invoice_number"));
    db.Invoices.Where(x => x.Id == id).Update(x => new Invoice { Number = newNumber });
    db.SaveChanges();
    increment_next_number();
    return invoice.Number;
  }

  /**
 * @since  2.7.0
 *
 * Increment the invoies next nubmer
 *
 * @return void
 */
  private void increment_next_number()
  {
    // Update next invoice number in settings


    db.Options.Where(x => x.Name == "next_invoice_number").Update(x => new Option { Value = Convert.ToString(Convert.ToInt32(x.Value) + 1) });
    db.SaveChanges();
  }

  /**
     * Log invoice activity to database
     * @param  mixed  id   invoiceid
     * @param  string  description activity description
     */
  public void log(int id, string description = "", bool client = false, string additional_data = "")
  {
    var staffid = "";
    var full_name = "";
    if (self.helper.defined("CRON"))
    {
      staffid = "[CRON]";
      full_name = "[CRON]";
    }
    else if (self.helper.defined("STRIPE_SUBSCRIPTION_INVOICE"))
    {
      staffid = null;
      full_name = "[Stripe]";
    }
    else if (client)
    {
      staffid = null;
      full_name = "";
    }
    else
    {
      var temp = staff_user_id;
      staffid = $"{temp}";
      full_name = self.helper.get_staff_full_name(temp);
    }

    db.SalesActivities.Add(new SalesActivity
    {
      Description = description,
      Date = DateTime.Now,
      RelId = id,
      RelType = "invoice",
      StaffId = staffid,
      FullName = full_name,
      AdditionalData = additional_data
    });
  }

  public Itemable get_invoice_item(int id)
  {
    return db.Itemables.First(x => x.Id == id);
  }

  public bool mark_as_cancelled(int id)
  {
    var isDraft = is_draft(id);
    db.Invoices.Where(x => x.Id == id).UpdateAsync(x => new Invoice { Status = InvoiceStatus.STATUS_CANCELLED, Sent = 1 });
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    if (isDraft) change_invoice_number_when_status_draft(id);
    log(id, "invoice_activity_marked_as_cancelled");
    self.hooks.do_action("invoice_marked_as_cancelled", id);
    return true;
  }

  public bool unmark_as_cancelled(int id)
  {
    db.Invoices.Where(x => x.Id == id).Update(x => new Invoice { Status = InvoiceStatus.STATUS_UNPAID });
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log(id, "invoice_activity_unmarked_as_cancelled");
    self.hooks.do_action("invoice_unmarked_as_cancelled", id);
    return true;
  }

  /**
 * Get this invoice generated recurring invoices
 * @since  Version 1.0.1
 * @param  mixed  id main invoice id
 * @return array
 */
  public List<Invoice?> get_invoice_recurring_invoices(int id)
  {
    return db.Invoices.Where(x => x.IsRecurringFrom == id).Select(invoice => get(invoice.Id)).ToList();
  }

  /**
     * Get invoice total from all statuses
     * @since  Version 1.0.2
     * @param  mixed  data $_POST data
     * @return array
     */
  public async Task<InvoiceTotalsResult> GetInvoicesTotalAsync(InvoicePostData data)
  {
    // Assuming you have a service for fetching currency and project info
    // var currencyId = await get_currency(data);
    var currency = db.Currencies.First(x => x.Id == data.Currency);
    var currencyId = currency.Id;

    var result = new InvoiceTotalsResult
    {
      Due = new List<double>(),
      Paid = new List<double>(),
      Overdue = new List<double>()
    };

    var hasPermissionView = self.helper.has_permission("invoices", "view");
    var hasPermissionViewOwn = self.helper.has_permission("invoices", "view_own");
    var allowStaffViewInvoicesAssigned = db.get_option("allow_staff_view_invoices_assigned");
    var noPermissionsQuery = self.helper.get_invoices_where_sql_for_staff(staff_user_id);

    for (var i = 1; i <= 3; i++)
    {
      var query = db.Invoices
        .Where(inv => inv.CurrencyId == currencyId && inv.Status != InvoiceStatus.STATUS_CANCELLED && inv.Status != InvoiceStatus.STATUS_DRAFT);

      if (data.ProjectId.HasValue)
        query = query.Where(inv => inv.ProjectId == data.ProjectId.Value);
      else if (data.CustomerId.HasValue) query = query.Where(inv => inv.ClientId == data.CustomerId.Value);

      query = i switch
      {
        3 => query.Where(inv => inv.Status == InvoiceStatus.STATUS_OVERDUE),
        1 => query.Where(inv => inv.Status != InvoiceStatus.STATUS_PAID),
        _ => query
      };

      query = data.Years != null && data.Years.Any()
        ? query.Where(inv => data.Years.Contains(DateTime.Parse(inv.Date).Year))
        : query.Where(inv => DateTime.Parse(inv.Date).Year == DateTime.Now.Year);

      // if (!hasPermissionView)
      // {
      //   query = query.Where(inv => EF.Functions.Like(inv.SaleAgentNavigation.Id, noPermissionsQuery));
      // }
      var invoices = query.Select(inv => new
      {
        inv.Id,
        inv.Total,
        Outstanding = i == 1
          ? inv.Total - db.InvoicePaymentRecords.Where(pr => pr.InvoiceId == inv.Id).Select(pr => Convert.ToInt32(pr.Amount)).Sum()
                      - db.Credits.Where(c => c.InvoiceId == inv.Id).Sum(c => c.Amount)
          : 0,
        TotalPaid = i == 2 ? db.InvoicePaymentRecords.Where(pr => pr.InvoiceId == inv.Id).Select(pr => Convert.ToInt32(pr.Amount)).Sum() : 0
      }).ToList();

      foreach (var invoice in invoices)
        if (i == 1)
          result.Due.Add(invoice.Outstanding);
        else if (i == 2)
          result.Paid.Add(invoice.TotalPaid);
        else if (i == 3) result.Overdue.Add(invoice.Total);
    }

    currency = self.helper.get_currency(currencyId);
    result.DueTotal = result.Due.Sum();
    result.PaidTotal = result.Paid.Sum();
    result.OverdueTotal = result.Overdue.Sum();
    result.Currency = currency.Symbol;
    result.CurrencyId = currencyId;
    return result;
  }

  public async Task<List<Expense>> get_expenses_to_bill(int client_id)
  {
    // var where = 'billable=1 AND client_id='. $client_id. ' AND invoiceid IS NULL';
    var where = CreateCondition<Expense>(x => x.Billable == true && x.ClientId == client_id && x.InvoiceId == null);
    var can_view_expenses = self.helper.has_permission("expenses", "", "view");
    var output = expenses_model.get(where);
    if (!can_view_expenses) output = output.Where(x => x.AddedFrom == staff_user_id).ToList();
    return output;
  }

  /**
     * Delete invoice items and all connections
     * @param  mixed  id invoiceid
     * @return boolean
     */
  public bool delete(int id, bool simpleDelete = false)
  {
    if (db.get_option_compare("delete_only_on_last_invoice", 1) &&
        simpleDelete == false &&
        !string.IsNullOrEmpty(is_last_invoice(id)))
      return false;

    var number = self.helper.format_invoice_number(id);
    var isDraft = is_draft(id);

    self.hooks.do_action("before_invoice_deleted", id);

    var invoice = get(id);


    var affected_rows = db.Invoices.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;

    if (!string.IsNullOrEmpty(invoice.ShortLink))
      self.helper.app_archive_short_link(invoice.ShortLink);
    if (db.get_option_compare("invoice_number_decrement_on_delete", 1) &&
        db.get_option_compare("next_invoice_number", 1) &&
        simpleDelete == false &&
        !isDraft)
      // Decrement next invoice number to
      decrement_next_number();
    if (simpleDelete == false)
    {
      db.Expenses.Where(x => x.InvoiceId == id).Update(x => new Expense { InvoiceId = null });
      db.Proposals.Where(x => x.InvoiceId == id).Update(x => new Proposal { InvoiceId = null, DateConverted = null });
      db.Tasks.Where(x => x.InvoiceId == id).Update(x => new Task { InvoiceId = null, Billed = 0 });


      // if is converted from estimate set the estimate invoice to null
      if (db.Estimates.Any(x => x.InvoiceId == id))
      {
        var estimate = db.Estimates.FirstOrDefault(x => x.InvoiceId == id);
        db.Estimates
          .Where(x => x.Id == estimate.Id)
          .Update(x => new Estimate
          {
            InvoiceId = null,
            InvoicedDate = null
          });

        estimates_model.log_estimate_activity(estimate.Id, "not_estimate_invoice_deleted");
      }
    }

    db.CreditNotes
      .Where(x =>
        db.Credits.Any(y => y.InvoiceId == x.Id)
      )
      .ToList()
      .ForEach(credit_note => { credit_notes_model.update_credit_note_status(credit_note.Id); });

    db.Credits
      .Where(x => x.InvoiceId == id)
      .Delete();

    db.Notes.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    delete_tracked_emails(id, "invoice");
    db.Taggables.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    db.Reminders.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    db.ViewsTrackings.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    db.CustomFieldsValues
      .Where(x =>
        db.Itemables.Any(y => y.RelId == x.Id && y.RelType == "invoice") &&
        x.FieldTo == "invoice"
      )
      .Delete();
    db.Itemables.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    self.helper.get_items_by_type("invoice", id).ForEach(item =>
      db.RelatedItems.Where(x => x.ItemId == item.Id).Delete()
    );
    db.ScheduledEmails.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    db.InvoicePaymentRecords.Where(x => x.InvoiceId == id).Delete();
    db.SalesActivities.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    db.Invoices.Where(x => x.IsRecurringFrom == id).Update(x => new Invoice { IsRecurringFrom = null });

    // Delete the custom field values
    db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "invoice").Delete();
    db.ItemTaxes.Where(x => x.RelId == id && x.RelType == "invoice").Delete();

    // Get billed tasks for this invoice and set to unbilled
    db.Tasks
      .Where(x => x.InvoiceId == id)
      .ToList()
      .ForEach(task =>
      {
        db.Tasks.Update(new Task
        {
          InvoiceId = null,
          Billed = 0
        });
      });


    get_attachments(x => x.Id == id)
      .ToList()
      .ForEach(attachment => delete_attachment(attachment.Id));
    var attachments = get_attachments(x => x.Id == id);
    attachments.ForEach(attachment => delete_attachment(attachment.Id));
    // Get related tasks
    db.Tasks
      .Where(x => x.RelId == id && x.RelType == "invoice")
      .ToList()
      .ForEach(task => tasks_model.delete_task(task.Id));
    if (simpleDelete == false) log_activity("Invoice Deleted [" + number + "]");
    self.hooks.do_action("after_invoice_deleted", id);
    return true;
  }

  /**
     *  Delete invoice attachment
     * @since  Version 1.0.4
     * @param   mixed id  attachmentid
     * @return  boolean
     */
  public bool delete_attachment(int id)
  {
    var attachment = get_attachments(x => x.Id == id).FirstOrDefault();
    var deleted = false;
    if (attachment == null) return deleted;
    if (string.IsNullOrEmpty(attachment.External))
      self.helper.unlink(self.helper.get_upload_path_by_type("invoice") + attachment.RelId + "/" + attachment.FileName);
    var affected_rows = db.Files.Where(x => x.Id == attachment.Id).Delete();
    if (affected_rows > 0)
    {
      deleted = true;
      log_activity("Invoice Attachment Deleted [InvoiceID: " + attachment.RelId + "]");
    }

    if (!self.helper.is_dir(self.helper.get_upload_path_by_type("invoice") + attachment.RelId)) return deleted;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = self.helper.list_files(self.helper.get_upload_path_by_type("invoice") + attachment.RelId);
    if (!other_attachments.Any())
      // okey only index.html so we can delete the folder also
      self.helper.delete_dir(self.helper.get_upload_path_by_type("invoice") + attachment.RelId);

    return deleted;
  }

  /**
   * @since  2.7.0
   *
   * Decrement the invoies next number
   *
   * @return void
   */
  public void decrement_next_number()
  {
    db.Options
      .Where(x => x.Name == "next_invoice_number")
      .Update(x => new Option
      {
        Value = Convert.ToString(Convert.ToInt32(x.Value) - 1)
      });
  }

  /**
     * Insert new invoice to database
     * @param array  data invoice data
     * @return mixed - false if not insert, invoice ID if succes
     */
  public int add(InvoiceOption data, bool expense = false)
  {
    var insert = new Invoice();
    insert.Prefix = db.get_option("invoice_prefix");
    insert.NumberFormat = db.get_option<int>("invoice_number_format");
    insert.DateCreated = DateTime.Now;
    var save_and_send = isset(data, "save_and_send");
    insert.AddedFrom = !self.helper.defined("CRON") ? staff_user_id : 0;
    insert.CancelOverdueReminders = isset(data, "cancel_overdue_reminders") ? 1 : 0;
    insert.AllowedPaymentModes = isset(data, "allowed_payment_modes") ? JsonConvert.SerializeObject(data.AllowedPaymentModes) : JsonConvert.SerializeObject(new { });

    var billed_tasks = isset(data, "billed_tasks")
      ? data.billed_tasks
      : new List<Task>();

    var billed_expenses = isset(data, "billed_expenses")
      ? data.billed_expenses
      : new List<Expense>();

    var invoices_to_merge = isset(data, "invoices_to_merge") ? data.invoices_to_merge : new List<int>();
    var cancel_merged_invoices = isset(data, "cancel_merged_invoices");

    var tags = isset(data, "tags")
      ? data.tags
      : new List<Taggable>();

    if (data.save_as_draft)
      insert.Status = InvoiceStatus.STATUS_DRAFT;
    else if (data.save_and_send_later) insert.Status = InvoiceStatus.STATUS_DRAFT;

    if (!string.IsNullOrEmpty(data.Recurring))
    {
      if (data.Recurring == "custom")
      {
        insert.RecurringType = data.repeat_type_custom;
        insert.CustomRecurring = 1;
        insert.Recurring = data.repeat_every_custom;
      }
    }
    else
    {
      insert.CustomRecurring = 0;
      insert.Recurring = string.Empty;
    }

    var custom_fields = new List<CustomField>();
    if (isset(data, "custom_fields")) custom_fields = data.custom_fields;

    insert.Hash = self.helper.uuid();
    var items = new List<ItemableOption>();

    if (isset(data, "newitems"))
      items = data.newitems;
    data = (InvoiceOption)map_shipping_columns(data, expense);

    if (isset(insert, "ShippingStreet"))
    {
      insert.ShippingStreet = insert.ShippingStreet.Trim();
      insert.ShippingStreet = insert.ShippingStreet.nl2br();
    }

    insert.BillingStreet = data.BillingStreet.Trim();
    insert.BillingStreet = data.BillingStreet.nl2br();

    if (data.Status.HasValue && data.Status == InvoiceStatus.STATUS_DRAFT)
      insert.Number = InvoiceStatus.STATUS_DRAFT_NUMBER;

    insert.DueDate = isset(data, "duedate") && string.IsNullOrEmpty(data.DueDate) ? null : data.DueDate;
    var hook = self.hooks.apply_filters("before_invoice_added", new
    {
      data,
      items
    });
    data = hook.data;
    items = hook.items;
    var result = db.Invoices.Add(insert);
    db.SaveChanges();
    var insert_id = result.Entity.Id;
    if (result.IsAdded()) return 0;
    if (custom_fields.Any())
      self.helper.handle_custom_fields_post(insert_id, custom_fields);
    self.helper.handle_tags_save(tags, insert_id, "invoice");

    foreach (var m in invoices_to_merge)
    {
      var merged = false;
      var or_merge = get(m);
      if (cancel_merged_invoices == false)
      {
        if (delete(m, true)) merged = true;
      }
      else
      {
        if (mark_as_cancelled(m))
        {
          merged = true;
          var admin_note = or_merge.AdminNote;
          var note = "Merged into invoice " + self.helper.format_invoice_number(insert_id);
          if (!string.IsNullOrEmpty(admin_note))
            admin_note += "\n\r" + note;
          else
            admin_note = note;

          db.Invoices
            .Where(x => x.Id == m)
            .Update(x => new Invoice { AdminNote = admin_note });
          db.SaveChanges();
// var items = new List<Item>();
          items.ForEach(item => db.RelatedItems
            .Where(x => x.ItemId == item.Id)
            .Delete());
        }
      }

      if (!merged) continue;

      var is_expense_invoice = db.Expenses.FirstOrDefault(x => x.InvoiceId == or_merge.Id);
      if (is_expense_invoice != null) db.Expenses.Where(x => x.InvoiceId == is_expense_invoice.Id).Update(x => new Expense { InvoiceId = insert_id });
      if (db.Estimates.Any(x => x.InvoiceId == or_merge.Id))
      {
        var estimate = db.Estimates.FirstOrDefault(x => x.InvoiceId == or_merge.Id);
        db.Estimates.Where(x => x.Id == estimate.Id).Update(x => new Estimate()
        {
          InvoiceId = insert_id
        });
      }
      else if (db.Proposals.Any(x => x.InvoiceId == or_merge.Id))
      {
        var proposal = db.Proposals.First(x => x.InvoiceId == or_merge.Id);
        db.Proposals.Where(x => x.Id == proposal.Id).Update(x => new Proposal
        {
          InvoiceId = insert_id
        });
      }
    }

    billed_tasks
      .ForEach(task =>
      {
        var taskUpdateData = new Task()
        {
          Billed = 1,
          InvoiceId = insert_id
        };
        if (task.Status != TaskStatus.STATUS_COMPLETE)
        {
          taskUpdateData.Status = TaskStatus.STATUS_COMPLETE;
          taskUpdateData.DateFinished = DateTime.Now;
        }

        db.Tasks.Where(x => x.Id == task.Id)
          .Update(x => taskUpdateData);
        db.SaveChanges();
      });

    billed_expenses
      .Select(x => x.Id)
      .ToList()
      .ForEach(expense_id => db.Expenses
        .Where(x => x.Id == expense_id)
        .Update(x => new Expense { InvoiceId = insert_id })
      );

    self.helper.update_invoice_status(insert_id);

    // Update next invoice number in settings if status is not draft
    if (!is_draft(insert_id)) increment_next_number();

    foreach (var kvp in items)
    {
      var key = "";
      var item = itemable(kvp);
      var itemid = self.helper.add_new_sales_item_post(item, insert_id, "invoice");
      if (itemid <= 0) continue;
      // if (isset(billed_tasks, key))
      //   foreach (var _task_id in billed_tasks[key])
      //     db.RelatedItems.Add(new RelatedItem
      //     {
      //       ItemId = itemid,
      //       RelId = _task_id,
      //       RelType = "task"
      //     });
      // else if (isset(billed_expenses, key))
      //   foreach (var _expense_id in billed_expenses[key])
      //     db.RelatedItems.Add(new RelatedItem
      //     {
      //       ItemId = itemid,
      //       RelId = _expense_id,
      //       RelType = "expense"
      //     });

      self.helper.maybe_insert_post_item_tax(itemid, convert<PostItem>(item), insert_id, "invoice");
    }

    self.helper.update_sales_total_tax_column(insert_id, "invoice", "invoices");
    var lang_key = "";
    if (!self.helper.defined("CRON") && expense == false)
      lang_key = "invoice_activity_created";
    else if (!self.helper.defined("CRON") && expense)
      lang_key = "invoice_activity_from_expense";
    else if (self.helper.defined("CRON") && expense == false)
      lang_key = "invoice_activity_recurring_created";
    else
      lang_key = "invoice_activity_recurring_from_expense_created";
    log_invoice_activity(insert_id, lang_key);
    if (save_and_send)
      send_invoice_to_client(insert_id, "", true, "", true);
    self.hooks.do_action("after_invoice_added", insert_id);

    return insert_id;
  }

  /**
     * Send invoice to client
     * @param  mixed  id        invoiceid
     * @param  string  template  email template to sent
     * @param  boolean attachpdf attach invoice pdf or not
     * @return boolean
     */
  public bool send_invoice_to_client(int id, string template_name = "", bool attachpdf = true, string cc = "", bool manually = false, dynamic attachStatement = default)
  {
    var isDraft = false;
    var originalNumber = 0;
    if (isDraft = is_draft(id))
      // Update invoice number from draft before sending
      originalNumber = change_invoice_number_when_status_draft(id);

    var invoice = get(id);
    invoice = self.hooks.apply_filters("invoice_object_before_send_to_client", invoice);

    if (template_name == "")
    {
      template_name = invoice.Sent == 0 ? "invoice_send_to_customer" : "invoice_send_to_customer_already_sent";

      template_name = self.hooks.apply_filters("after_invoice_sent_template_statement", template_name);
    }

    var emails_sent = new List<int>();
    var send_to = new List<int>();

    // Manually is used when sending the invoice via add/edit area button Save & Send

    if (!self.helper.defined("CRON") && manually == false)
    {
      send_to = self.input.post<string>("sent_to").Split(",").ToList().Select(x => Convert.ToInt32(x)).ToList();
    }
    else if (self.globals("scheduled_email_contacts") != null)
    {
      send_to = new List<int>() { self.globals<int>("scheduled_email_contacts") };
    }
    else
    {
      var contacts = get_contacts_for_invoice_emails(invoice.ClientId);
      send_to = contacts.Select(x => x.Id).ToList();
    }

    var statementPdfFileName = string.Empty;
    var status_updated = string.Empty;
    var attachStatementPdf = false;
    if (send_to.Any())
    {
      if (isset(attachStatement, "attach") && attachStatement.attach == true)
      {
        var statement = convert<StatementResult>(clients_model.get_statement(invoice.ClientId, attachStatement.from, attachStatement.to));
        var statementPdf = PdfHelper.statement_pdf(self.helper, statement);
        statementPdfFileName = slug_it(self.helper.label("customer_statement") + "-" + statement.Client.company);

        attachStatementPdf = statementPdf.Output(statementPdfFileName + ".pdf");
      }

      status_updated = self.helper.update_invoice_status(invoice.Id, true, true);

      var invoice_number = self.helper.format_invoice_number(invoice.Id);

      if (attachpdf)
      {
        self.helper.set_mailing_constant();
        var pdf = self.helper.invoice_pdf(get(id));
        var attach = pdf.Output(invoice_number + ".pdf");
      }

      var i = 0;
      foreach (var contact_id in send_to)
      {
        if (contact_id != 0)
        {
          // Send cc only for the first contact
          if (!string.IsNullOrEmpty(cc) && i > 0) cc = "";
          var contact = clients_model.get_contact(contact_id);
          if (contact == null) continue;

          // var template = self.helper.mail_template(template_name, invoice, contact, cc);
          var template = mail_template(template_name, invoice, contact, cc);

          if (attachpdf)
            template.add_attachment(new MailAttachment()
            {
              attachment = attachpdf,
              filename = (invoice_number + ".pdf").Replace("/", "-"),
              type = "application/pdf"
            });

          if (attachStatementPdf)
            template.add_attachment(new MailAttachment()
            {
              attachment = attachStatementPdf,
              filename = statementPdfFileName + ".pdf",
              type = "application/pdf"
            });

          if (template.send())
          {
            var sent = true;
            emails_sent.Add(int.Parse(contact.Email));
          }
        }

        i++;
      }
    }

    else if (isDraft)
    {
      // Revert the number on failure

      db.Invoices.Where(x => x.Id == id)
        .Update(x => new Invoice { Number = originalNumber });

      decrement_next_number();

      return false;
    }

    if (emails_sent.Any())
    {
      set_invoice_sent(id, false, emails_sent.Select(x => Convert.ToInt32(x)).ToList(), true);
      self.hooks.do_action("invoice_sent", id);
      return true;
    }

    // In case the invoice not sent and the status was draft and
    // the invoice status is updated before send return back to draft status
    // and actually update the number to the orignal number
    if (!isDraft || string.IsNullOrEmpty(status_updated)) return false;
    decrement_next_number();
    db.Invoices
      .Where(x => x.Id == invoice.Id)
      .Update(x => new Invoice
      {
        Status = InvoiceStatus.STATUS_DRAFT,
        Number = originalNumber
      });
    db.SaveChanges();
    return true;
  }

  /**
     * Log invoice activity to database
     * @param  mixed  id   invoiceid
     * @param  string  description activity description
     */
  public void log_invoice_activity(int id, string description = "", bool is_client = false, string additional_data = "")
  {
    var staffid = string.Empty;
    var full_name = string.Empty;
    if (self.helper.defined("CRON"))
    {
      staffid = "[CRON]";
      full_name = "[CRON]";
    }
    else if (self.helper.defined("STRIPE_SUBSCRIPTION_INVOICE"))
    {
      staffid = null;
      full_name = "[Stripe]";
    }
    else if (is_client)
    {
      staffid = null;
      full_name = string.Empty;
    }
    else
    {
      staffid = $"{staff_user_id}";
      full_name = self.helper.get_staff_full_name(self.helper.get_staff_user_id());
    }

    db.SalesActivities.Add(new SalesActivity
    {
      Description = description,
      Date = DateTime.Now,
      RelId = id,
      RelType = "invoice",
      StaffId = staffid,
      FullName = full_name,
      AdditionalData = additional_data
    });

    db.SaveChanges();
  }

  /**
     * Set invoice to sent when email is successfuly sended to client
     * @param mixed  id invoiceid
     * @param  mixed  manually is staff manually marking this invoice as sent
     * @return  boolean
     */
  public int set_invoice_sent(int id, bool manually = false, List<int> emails_sent = default, bool is_status_updated = false)
  {
    var result = db.Invoices
      .Where(x => x.Id == id)
      .Update(x => new Invoice
      {
        Sent = 1,
        DateSend = DateTime.Now
      });

    var marked = db.SaveChanges();

    if (marked > 0 && is_draft(id)) change_invoice_number_when_status_draft(id);
    var description = string.Empty;
    var additional_activity_data = string.Empty;
    if (self.helper.defined("CRON"))
    {
      additional_activity_data = JsonConvert.SerializeObject(new[]
      {
        "<custom_data>" + string.Join(", ", emails_sent) + "</custom_data>"
      });
      description = "invoice_activity_sent_to_client_cron";
    }
    else
    {
      if (manually == false)
      {
        additional_activity_data = JsonConvert.SerializeObject(new[]
        {
          "<custom_data>" + string.Join(", ", emails_sent) + "</custom_data>"
        });
        description = "invoice_activity_sent_to_client";
      }
      else
      {
        additional_activity_data = JsonConvert.SerializeObject(new List<string>());
        description = "invoice_activity_marked_as_sent";
      }
    }

    if (is_status_updated == false) self.helper.update_invoice_status(id, true);

    db.ScheduledEmails.Where(x => x.RelId == id && x.RelType == "invoice").Delete();
    log_invoice_activity(id, description, false, additional_activity_data);
    return marked;
  }


  /**
       * Map the shipping columns into the data
       *
       * @param  array   data
       * @param  boolean  expense
       *
       * @return array
       */
  private Invoice map_shipping_columns(InvoiceOption data, bool expense = false)
  {
    if (data.IncludeShipping)
    {
      shipping_fields.ForEach((s_field) =>
      {
        var inv = invoice(data);
        // if (isset(data, s_field))
        //   data[s_field] = null;
        data.ShowShippingOnInvoice = true;
        data.IncludeShipping = false;
      });
    }
    else
    {
      if (self.helper.defined("CRON") || expense) return data;
      data.IncludeShipping = true;
      data.ShowShippingOnInvoice = data.ShowShippingOnInvoice;
    }

    return data;
  }

  /**
     * Get the contacts that should receive invoice related emails
     *
     * @param  int $client_id
     *
     * @return array
     */
  protected List<Contact> get_contacts_for_invoice_emails(int client_id)
  {
    // return this.clients_model.get_contacts(client_id,
    // 'active' => 1, 'invoice_emails' => 1,
    //   ]);
    return clients_model.get_contacts(x => x.Id == client_id && x.Active && x.InvoiceEmails == 1, x => true);
  }
}
