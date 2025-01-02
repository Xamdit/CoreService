using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Library.Merger;
using Service.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Sale;
using Service.Models.Client;
using Service.Models.Contracts;
using Service.Models.Invoices;
using Service.Models.Misc;
using Service.Models.Payments;
using File = Service.Entities.File;
using static Service.Helpers.Template.TemplateHelper;

namespace Service.Models.CreditNotes;

public class CreditNotesModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private InvoicesModel invoices_model = self.invoices_model(db);
  private ClientsModel clients_model = self.clients_model(db);
  private MiscModel misc_model = self.misc_model(db);
  private PaymentModesModel payment_modes_model = self.payment_modes_model(db);

  public List<CreditNoteOption> get_statuses()
  {
    return hooks.apply_filters("before_get_credit_notes_statuses", new List<CreditNoteOption>
    {
      new()
      {
        id = 1,
        color = "#03a9f4",
        name = label("credit_note_status_open"),
        order = 1,
        filter_default = true
      },
      new()
      {
        id = 2,
        color = "#84c529",
        name = label("credit_note_status_closed"),
        order = 2,
        filter_default = true
      },
      new()
      {
        id = 3,
        color = "#777",
        name = label("credit_note_status_void"),
        order = 3,
        filter_default = false
      }
    });
  }

  public async Task<List<CreditNoteResult>> get_available_creditable_invoices(int credit_note_id)
  {
    var has_permission_view = db.has_permission("invoices", "", "view");
    var _invoices_statuses_available_for_credits = this.invoices_statuses_available_for_credits();
    var credit_note = db.CreditNotes.FirstOrDefault(x => x.Id == credit_note_id);
    var invoice_query = db.Invoices
      .Include(x => x.Currency)
      .Where(x =>
        x.ClientId == credit_note.ClientId &&
        _invoices_statuses_available_for_credits.Contains(x.Status ?? 0)
      );


    if (!has_permission_view)
      invoice_query = invoice_query
        .Where(x =>
          x.AddedFrom == db.get_staff_user_id()
        );
    var invoices = invoice_query.ToList();
    var output = invoices
      .Select(x =>
        new CreditNoteResult
        {
          Invoice = x,
          TotalLeftToPay = self.helper.get_invoice_total_left_to_pay(x.Id, x.Total)
        })
      .ToList();


    return output;
  }

  /**
  * Send credit note to client
  * @param  mixed   id        credit note id
  * @param  string   template  email template to sent
  * @param  boolean  attachpdf attach credit note pdf or not
  * @return boolean
  */
  public bool send_credit_note_to_client(int id, bool attachpdf = true, string cc = "", bool manually = false)
  {
    var credit_note = get(x => x.Id == id).First();
    var number = db.format_credit_note_number(credit_note.Id);
    var sent = false;
    var sent_to = new List<int>();

    if (manually)
    {
      var contacts = clients_model.get_contacts(x => x.Id == credit_note.ClientId, x => x.Active && x.CreditNoteEmails == 1);
      sent_to = contacts
        .Select(x => x.Id)
        .ToList();
    }

    var attach = false;
    if (sent_to.Any())
    {
      if (attachpdf)
      {
        self.helper.set_mailing_constant();
        var pdf = self.helper.credit_note_pdf(credit_note);
        attach = pdf.Output($"{number}.pdf");
      }

      var i = 0;
      sent_to.ForEach(contact_id =>
      {
        if (contact_id > 0)
        {
          if (!string.IsNullOrEmpty(cc) && i > 0) cc = "";
          var contact = clients_model.get_contact(contact_id);
          var template = mail_template("credit_note_send_to_customer", credit_note, contact, cc);

          if (attachpdf)
            template.add_attachment(new MailAttachment()
            {
              attachment = attach,
              filename = $"{number}.pdf".Replace("/", "_"),
              type = "application/pdf"
            });

          if (template.send()) sent = true;
        }

        i++;
      });
    }
    else
    {
      return false;
    }

    if (!sent) return false;
    hooks.do_action("credit_note_sent", id);
    return true;
  }

  /**
   * Get credit note/s
   * @param  mixed  id    credit note id
   * @param  array   where perform where
   * @return mixed
   */
  public List<CreditNote> get(Expression<Func<CreditNote, bool>> where)
  {
    return db.CreditNotes.Include(x => x.Currency)
      .Where(where)
      .OrderByDescending(x => x.Number)
      .ThenByDescending(x => x.Date)
      .ToList();
  }

  public CreditNote? get(int id, Expression<Func<CreditNote, bool>> where)
  {
    var credit_note = db.CreditNotes
      .Include(x => x.Currency)
      .Where(where)
      .FirstOrDefault(x => x.Id == id);

    if (credit_note == null) return credit_note;

    var credit_note_result = new CreditNoteResult();
    credit_note_result.CreditNote = credit_note;
    credit_note_result.CreditNoteRefunds = get_refunds(id);
    credit_note_result.total_refunds = total_refunds_by_credit_note(id);
    credit_note_result.applied_credits = get_applied_credits(id);
    credit_note_result.remaining_credits = total_remaining_credits_by_credit_note(id);
    credit_note_result.credits_used = total_credits_used_by_credit_note(id);
    credit_note_result.items = db.get_items_by_type("credit_note", id);
    credit_note_result.Client = clients_model.get(x => x.Id == credit_note.ClientId).First();

    if (credit_note_result.Client == null)
    {
      credit_note_result.Client = new Entities.Client();
      credit_note_result.Client.Company = credit_note.DeletedCustomerName;
    }

    credit_note_result.attachments = get_attachments(id);

    return credit_note;
  }

  // public int add((CreditNote creditNote, List<CustomField>custom_fields, CreditNoteOption option, List<Itemable> newitems) data)
  public int add(CreditNoteDto data)
  {
    var save_and_send = data.option.save_and_send;
    data.creditNote.Prefix = db.get_option("credit_note_prefix");
    data.creditNote.NumberFormat = Convert.ToInt32(db.get_option("credit_note_number_format"));
    data.creditNote.DateCreated = DateTime.Now;
    data.creditNote.AddedFrom = db.get_staff_user_id();

    var items = data.newitems;
    var custom_fields = data.custom_fields;

    data.creditNote = map_shipping_columns(data.creditNote);
    var hook = hooks.apply_filters("before_create_credit_note", new { data, items });

    db.CreditNotes.Add(data.creditNote);
    var insert_id = data.creditNote.Id;
    if (insert_id <= 0) return 0;
    // Update next credit note number in settings

    var result = db.Options
      .Where(x => x.Name == "next_credit_note_number")
      .Update(x => new Option { Value = x.Value + 1 });
    if (custom_fields.Any())
      self.helper.handle_custom_fields_post(insert_id, custom_fields);

    foreach (var item in items.Where(item => item.Id == db.add_new_sales_item_post(item!, insert_id, "credit_note")))
    {
      var item_taxes = item.ItemTaxes.ToList().Select(x => x.TaxName).ToList();
      var postItem = new PostItem
      {
        // TaxNames = items.Select(x => x.ItemTaxes.ToList()).ToList();
        TaxNames = item_taxes
      };
      db.maybe_insert_post_item_tax(item.Id, postItem, insert_id, "credit_note");
    }

    db.update_sales_total_tax_column(insert_id, "credit_note", "creditnotes");

    log_activity($"Credit Note Created [ID: {insert_id}]");

    hooks.do_action("after_create_credit_note", insert_id);

    if (save_and_send) send_credit_note_to_client(insert_id, true, manually: true);

    return insert_id;
  }

  /**
   * Update proposal
   * @param  mixed  data  _POST data
   * @param  mixed  id   proposal id
   * @return boolean
   */
  public bool update(CreditNoteDto data, int id)
  {
    var save_and_send = data.option.save_and_send;
    var items = data.newitems;
    var newitems = data.newitems;
    var custom_fields = data.custom_fields;

    // if (data.custom_fields)
    //   if (self.helper.handle_custom_fields_post(id, custom_fields))

    data.creditNote = map_shipping_columns(data.creditNote);
    dynamic hook = new
    {
      id,
      data.creditNote,
      items,
      newitems,
      removed_items = data.remove_items.Any() ? data.remove_items : new List<Itemable>()
    };
    hooks.apply_filters("before_update_credit_note", hook);

    data = hook.data;
    items = hook.items;
    newitems = hook.newitems;
    data.remove_items = hook.removed_items;

    // Delete items checked to be removed from database
    data.remove_items
      .Select(x => x.Id)
      .ToList()
      .ForEach(x => db.handle_removed_sales_item_post(x, "credit_note"))
      ;

    var affected_rows = db.CreditNotes.Where(x => x.Id == id).Update(x => data);


    foreach (var item in items)
    {
      //key => item
      // if (self.helper.update_sales_item_post(item.Id, item)) affectedRows++;
      // if (item.custom_fields) self.helper.handle_custom_fields_post(item.Id, item.custom_fields);

      var itemTax = item.ItemTaxes.FirstOrDefault();
      if (!string.IsNullOrEmpty(itemTax.TaxName))
      {
        db.delete_taxes_from_item(itemTax.Id, "credit_note");
      }
      else
      {
        var item_taxes = db.get_credit_note_item_taxes(item.Id);
        var _item_taxes_names = item_taxes.Select(_item_tax => _item_tax.TaxName).ToList();
        _item_taxes_names.ForEach(_item_tax =>
        {
          var dataset = new List<string>();
          dataset = items.Select(x => x.ItemTaxes.Select(y => y.TaxName)).Aggregate(dataset, (current, names) => (List<string>)TypeMerger.Merge(current, names.ToList()));
          if (!dataset.Contains(_item_tax)) return;
          db.ItemTaxes.Count(x => x.Id == Convert.ToInt32(_item_tax));
        });

        var i = 0;
        db.maybe_insert_post_item_tax(item.Id, convert<PostItem>(item.ItemTaxes), id, "credit_note");
      }
    }


    var result = newitems.Select(item =>
      {
        var new_item_added = db.add_new_sales_item_post(item, id, "credit_note");
        if (new_item_added <= 1) return false;
        db.maybe_insert_post_item_tax(new_item_added, convert<PostItem>(item), id, "credit_note");
        return true;
      })
      .Where(x => x)
      .ToList();


    if (save_and_send) send_credit_note_to_client(id, true, "", true);

    if (result.Any())
    {
      update_credit_note_status(id);
      db.update_sales_total_tax_column(id, "credit_note", "creditnotes");
    }

    log_activity($"Credit Note Updated [ID:{id}]");
    hooks.do_action("after_update_credit_note", id);
    return true;
  }

  /**
  *  Delete credit note attachment
  * @param   mixed  id  attachmentid
  * @return  boolean
  */
  public bool delete_attachment(int id)
  {
    var attachment = misc_model.get_file(id);
    var deleted = false;
    if (attachment == null) return deleted;
    if (string.IsNullOrEmpty(attachment.External))
      unlink($"{get_upload_path_by_type("credit_note")}{attachment.RelId}/{attachment.FileName}");

    var affected_rows = db.Files.Where(x => x.Id == id).Delete();
    if (affected_rows > 0)
    {
      deleted = true;
      log_activity($"Credit Note Attachment Deleted [Credite Note: {db.format_credit_note_number(attachment.RelId)}]");
    }

    if (!is_dir(get_upload_path_by_type("credit_note") + attachment.RelId)) return deleted;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = list_files(get_upload_path_by_type("credit_note") + attachment.RelId);
    if (other_attachments.Any())
      // okey only index.html so we can delete the folder also
      delete_dir(get_upload_path_by_type("credit_note") + attachment.RelId);

    return deleted;
  }

  public List<File> get_attachments(int credit_note_id)
  {
    return db.Files
      .Where(x => x.RelId == credit_note_id && x.RelType == "credit_note")
      .ToList();
  }

  /**
  * Delete credit note
  * @param  mixed  id credit note id
  * @return boolean
  */
  public bool delete(int id, bool simpleDelete = false)
  {
    hooks.do_action("before_credit_note_deleted", id);
    var result = db.CreditNotes.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;

    var current_credit_note_number = Convert.ToInt32(db.get_option("next_credit_note_number"));

    if (current_credit_note_number > 1 && simpleDelete == false && this.is_last_credit_note(id))
      // Decrement next credit note number
      db.Options.Where(x => x.Name == "next_credit_note_number")
        .Update(x => new Option { Value = Convert.ToString(Convert.ToInt32(x.Value) - 1) });

    delete_tracked_emails(id, "credit_note");

    // Delete the custom field values
    db.CustomFieldsValues
      .Where(x =>
        db.Itemables.Where(y => y.RelType == "credit_note" && y.RelId == id).Select(y => y.Id).Contains(x.RelId) &&
        x.FieldTo == "items"
      )
      .Delete();


    db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "credit_note").Delete();
    db.CreditNotes.Where(x => x.Id == id).Delete();
    db.CreditNoteRefunds.Where(x => x.CreditNoteId == id).Delete();
    db.Itemables.Where(x => x.RelType == "credit_note" && x.RelId == id).Delete();

    db.ItemTaxes.Where(x => x.RelId == id && x.RelType == "credit_note").Delete();

    var attachments = get_attachments(id);
    attachments.ForEach(attachment => { delete_attachment(attachment.Id); });

    db.Reminders.Where(x => x.RelId == id && x.RelType == "credit_note").Delete();
    hooks.do_action("after_credit_note_deleted", id);
    return true;
  }

  public bool mark(int id, int? status)
  {
    db.CreditNotes.Where(x => x.Id == id).Update(x => new CreditNote { Status = status });
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    hooks.do_action("credit_note_status_changed", id, new { status });
    return true;
  }

  public string total_remaining_credits_by_customer(int customer_id)
  {
    var has_permission_view = db.has_permission("credit_notes", "", "view");
    var query = db.CreditNotes.Where(x => x.ClientId == customer_id && x.Status == 1);
    if (!has_permission_view)
      query = query.Where(x => x.AddedFrom == db.get_staff_user_id());
    var credits = query.ToList();
    var total = calc_remaining_credits(credits);
    return total;
  }

  public string total_remaining_credits_by_credit_note(int credit_note_id)
  {
    var credits = db.CreditNotes.Where(x => x.Id == credit_note_id).ToList();

    var total = calc_remaining_credits(credits);

    return total;
  }

  private string calc_remaining_credits(List<CreditNote> credits)
  {
    var total = string.Empty;
    var credits_ids = new List<int>();


    foreach (var credit in credits.ToList())
    {
      total = bcadd($"{total}", $"{credit.Total}", get_decimal_places());
      credits_ids.Add(credit.Id);
    }

    if (!credits_ids.Any()) return total;
    var applied_credits = db.Credits.Where(x => credits_ids.Contains(x.CreditId)).ToList();
    applied_credits.ForEach(credit => { total = bcsub(total, $"{credit.Amount}", get_decimal_places()); });
    return credits_ids.Select(total_refunds_by_credit_note).Aggregate(total, (current, _total_refunds_by_credit_note) => bcsub(current, _total_refunds_by_credit_note.ToString(), get_decimal_places()));
  }

  public void delete_applied_credit(int id, int credit_id, int invoice_id)
  {
    db.Credits.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return;
    update_credit_note_status(credit_id);
    self.helper.update_invoice_status(invoice_id);
  }

  public async Task<int?> credit_note_from_invoice(int invoice_id)
  {
    // var _invoice = invoices_model.get(invoice_id);
    var _invoice = invoices_model.get(invoice_id);
    // var new_credit_note_data = new List<NewCreditNoteData>();
    var new_credit_note_data = new CreditNoteDto();
    new_credit_note_data.ClientId = _invoice.ClientId;
    new_credit_note_data.Number = Convert.ToInt32(db.get_option("next_credit_note_number"));
    new_credit_note_data.Date = DateTime.Now;
    new_credit_note_data.ShowQuantityAs = _invoice.ShowQuantityAs;
    new_credit_note_data.Currency = _invoice.Currency;
    new_credit_note_data.Subtotal = _invoice.Subtotal;
    new_credit_note_data.Total = _invoice.Total;
    new_credit_note_data.AdminNote = _invoice.AdminNote;
    new_credit_note_data.Adjustment = _invoice.Adjustment;
    new_credit_note_data.DiscountPercent = _invoice.DiscountPercent;
    new_credit_note_data.DiscountTotal = _invoice.DiscountTotal;
    new_credit_note_data.DiscountType = _invoice.DiscountType;
    new_credit_note_data.BillingStreet = clear_textarea_breaks(_invoice.BillingStreet);
    new_credit_note_data.BillingCity = _invoice.BillingCity;
    new_credit_note_data.BillingState = _invoice.BillingState;
    new_credit_note_data.BillingZip = _invoice.BillingZip;
    new_credit_note_data.BillingCountry = _invoice.BillingCountry;
    new_credit_note_data.ShippingStreet = clear_textarea_breaks(_invoice.ShippingStreet);
    new_credit_note_data.ShippingCity = _invoice.ShippingCity;
    new_credit_note_data.ShippingState = _invoice.ShippingState;
    new_credit_note_data.ShippingZip = _invoice.ShippingZip;
    new_credit_note_data.ShippingCountry = _invoice.ShippingCountry;
    new_credit_note_data.ReferenceNo = db.format_invoice_number(_invoice.Id);
    if (_invoice.IncludeShipping)
      new_credit_note_data.IncludeShipping = _invoice.IncludeShipping;
    new_credit_note_data.ShowShippingOnCreditNote = credit_note(_invoice).ShowShippingOnCreditNote;
    new_credit_note_data.ClientNote = db.get_option("predefined_clientnote_credit_note");
    new_credit_note_data.Terms = db.get_option("predefined_terms_credit_note");
    new_credit_note_data.AdminNote = "";
    new_credit_note_data.NewItems.Clear();
    var custom_fields_items = db.get_custom_fields("items");
    var key = 1;
    var itemables = new List<Itemable>();
    // foreach (var item in _invoice.Items)
    foreach (var item in itemables)

    {
      new_credit_note_data.NewItems[key].Description = item.Description;
      new_credit_note_data.NewItems[key].LongDescription = clear_textarea_breaks(item.LongDescription);
      new_credit_note_data.NewItems[key].Qty = item.Qty;
      new_credit_note_data.NewItems[key].Unit = item.Unit;
      var taxes = db.get_invoice_item_taxes(item.Id);
      new_credit_note_data.NewItems[key].TaxNames = taxes.Select(x => x.TaxName).ToList();
      new_credit_note_data.NewItems[key].Rate = item.Rate;
      new_credit_note_data.NewItems[key].ItemOrder = item.ItemOrder;
      foreach (var cf in custom_fields_items)
      {
        new_credit_note_data.NewItems[key].CustomFields.Items[cf.Id] = db.get_custom_field_value(item.Id, cf.Id, "items", false);
        if (!defined("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST"))
          self.helper.define("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST", true);
      }

      key++;
    }

    var id = add(new_credit_note_data);
    if (id == 0) return null;
    if (_invoice.Status != 2)
    {
      if (_invoice.Status == InvoiceStatus.STATUS_DRAFT) invoices_model.change_invoice_number_when_status_draft(invoice_id);
      var checker = await apply_credits(id, new Credit { InvoiceId = invoice_id, Amount = total_left_to_pay(credit_note(_invoice)) });
      if (checker > 0)
        self.helper.update_invoice_status(invoice_id, true);
    }

    log_activity($"Created Credit Note From Invoice [Invoice: {db.format_invoice_number(_invoice.Id)}, Credit Note: {db.format_credit_note_number(id)}]");
    hooks.do_action("created_credit_note_from_invoice", new { invoice_id, credit_note_id = id });
    return id;
  }

  public int create_refund(int id, CreditNoteRefund data)
  {
    if (data.Amount == 0) return 0;

    data.Note = data.Note.Trim();
    data.CreatedAt = DateTime.Now;
    data.CreditNoteId = id;
    data.StaffId = data.StaffId;
    data.RefundedOn = data.RefundedOn;
    data.PaymentMode = data.PaymentMode;
    data.Amount = data.Amount;
    data.Note = data.Note.nl2br();
    db.CreditNoteRefunds.Add(data);

    var insert_id = data.Id;
    if (insert_id <= 0) return insert_id;
    update_credit_note_status(id);
    hooks.do_action("credit_note_refund_created", new { data, credit_note_id = id });
    return insert_id;
  }

  public int edit_refund(int id, CreditNoteRefund data)
  {
    if (data.Amount == 0) return 0;

    var refund = get_refund(id);
    data.Note = data.Note.Trim();
    var affected_rows = db.CreditNoteRefunds
      .Where(x => x.Id == id)
      .Update(x => new CreditNoteRefund
      {
        RefundedOn = data.RefundedOn,
        PaymentMode = data.PaymentMode,
        Amount = data.Amount,
        Note = data.Note
      });


    if (affected_rows <= 0) return data.Id;
    update_credit_note_status(refund.CreditNoteId);
    hooks.do_action("credit_note_refund_updated", new { data, refund_id = refund.CreditNoteId });
    return data.Id;
  }

  public CreditNoteRefund? get_refund(int id)
  {
    return db.CreditNoteRefunds.FirstOrDefault(x => x.Id == id);
  }

  public List<CreditNoteRefund> get_refunds(int credit_note_id)
  {
    var refunds = db.CreditNoteRefunds
      .Include(x => x.CreditNote)
      .Include(x => x.PaymentMode)
      .Where(x => x.CreditNoteId == credit_note_id)
      .OrderByDescending(x => x.RefundedOn)
      .ToList();
    var payment_gateways = payment_modes_model.get_payment_gateways(true);
    var i = 0;
    refunds = refunds.Select(refund =>
      {
        if (string.IsNullOrEmpty(refund.PaymentMode))
          payment_gateways.ForEach(gateway =>
          {
            // if (refund.PaymentMode != gateway.Id) return null;
            var payment_modes_model = self.payment_modes_model(db);
            var row = payment_modes_model.get(x => x.Id == gateway.Id).First();
            refund.PaymentMode = row.Name;
          });
        i++;
        return refund;
      })
      .ToList();

    return refunds;
  }

  public bool delete_refund(int refund_id, int credit_note_id)
  {
    db.CreditNoteRefunds.Where(x => x.Id == refund_id).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    update_credit_note_status(credit_note_id);
    hooks.do_action("credit_note_refund_deleted", new { refund_id, credit_note_id });
    return true;
  }

  private int total_refunds_by_credit_note(int id)
  {
    return db.CreditNoteRefunds.Where(x => x.CreditNoteId == id).Sum(x => x.Amount);
  }

  public async Task<int> apply_credits(int id, Credit data)
  {
    if (data.Amount == 0) return 0;
    var dataset = new Credit
    {
      InvoiceId = data.InvoiceId,
      CreditId = id,
      StaffId = db.get_staff_user_id(),
      Date = DateTime.UtcNow,
      DateApplied = DateTime.UtcNow,
      Amount = data.Amount
    };
    await db.Credits.AddAsync(dataset);
    await db.SaveChangesAsync();

    var insert_id = dataset.Id;

    if (insert_id <= 0) return insert_id;
    update_credit_note_status(id);
    var invoice = db.Invoices
      .Include(x => x.Currency)
      .FirstOrDefault(x => x.Id == data.InvoiceId);
    if (invoice.Status == InvoiceStatus.STATUS_DRAFT)
      // update invoice number for invoice with draft - V2.7.2
      invoices_model.change_invoice_number_when_status_draft(invoice.Id);
    invoice = db.Invoices.Include(x => x.Currency).FirstOrDefault(x => x.Id == data.Id);
    var inv_number = db.format_invoice_number(data.InvoiceId);
    var credit_note_number = db.format_credit_note_number(id);
    invoices_model.log(data.InvoiceId, "invoice_activity_applied_credits", false, JsonConvert.SerializeObject(new[]
    {
      db.app_format_money(data.Amount, invoice.Currency.Name),
      credit_note_number
    }));

    hooks.do_action("credits_applied", new { data, credit_note_id = id });
    log_activity($"Credit Applied to Invoice [ Invoice: {inv_number}, Credit: {credit_note_number} ]");

    return insert_id;
  }

  private int total_credits_used_by_credit_note(int id)
  {
    return db.Credits.Where(x => x.CreditId == id).Sum(x => x.Amount);
  }

  public bool update_credit_note_status(int id)
  {
    var _total_refunds_by_credit_note = total_refunds_by_credit_note(id);
    var total_credits_used = total_credits_used_by_credit_note(id);

    var status = 1;

    // sum from table returns null if nothing found
    if (total_credits_used > 0 || _total_refunds_by_credit_note > 0)
    {
      var compare = total_credits_used + _total_refunds_by_credit_note;


      var credit = db.CreditNotes.FirstOrDefault(x => x.Id == id);

      if (credit != null)
        if (credit.Total == compare)
          status = 2;
    }


    db.CreditNotes.Where(x => x.Id == id).Update(x => new CreditNote { Status = status });
    var affected_rows = db.SaveChanges();
    return affected_rows > 0;
  }

  public async Task<List<CreditNote>> get_open_credits(int customer_id)
  {
    var query = db.CreditNotes.Where(x => x.ClientId == customer_id && x.Status == 1).AsQueryable();
    var has_permission_view = db.has_permission("credit_notes", "", "view");

    if (!has_permission_view) query = query.Where(x => x.AddedFrom == db.get_staff_user_id());

    var credits = query.ToList();

    // foreach ( credits as  key =>  credit) {
    //      credits[ key]['available_credits'] =  calculate_available_credits( credit.Id,  credit.Total);
    // }

    return credits;
  }

  public List<Credit> get_applied_invoice_credits(int invoice_id)
  {
    var rows = db.Credits.Where(x => x.InvoiceId == invoice_id).OrderByDescending(x => x.Date).ToList();
    return rows;
  }

  public List<Credit> get_applied_credits(int credit_id)
  {
    return db.Credits
      .Where(x => x.CreditId == credit_id)
      .OrderByDescending(x => x.Date)
      .ToList();
  }

  private string calculate_available_credits(int credit_id, double credit_amount = 0)
  {
    if (credit_amount == 0)
    {
      var row = db.CreditNotes.Where(x => x.Id == credit_id).FirstOrDefault();
      credit_amount = row.Total;
    }

    var available_total = $"{credit_amount}";

    // var bcsub = function_exists("bcsub");
    var applied_credits = get_applied_credits(credit_id);
    // foreach (var credit in applied_credits)
    //   if (bcsub)
    //     available_total = bcsub(available_total, credit.Amount, get_decimal_places());
    //   else
    //     available_total -= credit.Amount;

    var total_refunds = total_refunds_by_credit_note(credit_id);

    if (total_refunds == 0) return available_total;
    available_total = bcsub(available_total, $"{total_refunds}", get_decimal_places());
    // if (bcsub)
    //   available_total = bcsub(available_total, total_refunds, get_decimal_places());
    // else
    //   available_total -= total_refunds;
    return available_total;
  }

  public List<CreditNote> get_credits_years()
  {
    return db.CreditNotes
      // .Select(x=>new{year = x.Date.Year})
      .Distinct()
      .OrderByDescending(x => x.Date.Year)
      .ToList();
  }

  private CreditNote clear(CreditNote data)
  {
    data.ShippingStreet = string.Empty;
    data.ShippingCity = string.Empty;
    data.ShippingState = string.Empty;
    data.ShippingZip = string.Empty;
    data.ShippingCountry = null;

    return data;
  }


  private CreditNote map_shipping_columns(CreditNote data)
  {
    if (data.IncludeShipping is false)
    {
      data = clear(data);
      data.ShowShippingOnCreditNote = true;
      data.IncludeShipping = false;
    }
    else
    {
      data.IncludeShipping = true;
      // set by default for the next time to be checked
      data.ShowShippingOnCreditNote = data.ShowShippingOnCreditNote is true;
    }

    return data;
  }
}
