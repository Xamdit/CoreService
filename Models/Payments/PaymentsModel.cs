using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Service.Models.Client;
using Service.Models.Invoices;

namespace Service.Models.Payments;

public class PaymentsModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private readonly ClientsModel clients_model = self.clients_model(db);
  private readonly InvoicesModel invoices_model = self.invoices_model(db);
  private readonly PaymentModesModel paymentmodes_model = self.payment_modes_model(db);
  private readonly PaymentAttemptsModel paymentAttemptsModel = self.payment_attempts_model(db);

  public InvoicePaymentRecord get(int id)
  {
    var payment = db.InvoicePaymentRecords
      .Include(x => x.PaymentMode)
      .OrderBy(x => x.Id)
      .FirstOrDefault(x => x.Id == id);

    if (payment == null) return null;

    var paymentGateways = paymentmodes_model.get_payment_gateways(true);
    foreach (var gateway in paymentGateways.Where(gateway => payment.PaymentMode == gateway.Id.ToString()))
      payment.Name = gateway.Name;
    return payment;
  }

  public List<InvoicePaymentRecord> get_invoice_payments(int invoiceId)
  {
    var paymentGateways = paymentmodes_model.get_payment_gateways(true);
    var payments = db.InvoicePaymentRecords
      .Include(x => x.PaymentMode)
      .Where(x => x.InvoiceId == invoiceId)
      .OrderBy(x => x.Id)
      .ToList();
    payments = payments.Select(x =>
    {
      foreach (var gateway in paymentGateways.Where(gateway => x.PaymentMode == gateway.Id.ToString()))
        x.Name = gateway.Name;
      return x;
    }).ToList();
    return payments;
  }

  public int process_payment(InvoicePaymentRecord data, int invoiceId = 0, PaymentOption option = default)
  {
    if (!string.IsNullOrEmpty(data.PaymentMode)) return !db.is_staff_logged_in() ? 0 : add((data, option));

    // Invalid or missing payment mode
    if (string.IsNullOrEmpty(data.PaymentMode)) return 0;
    if (db.is_staff_logged_in() && db.has_permission("payments", "", "create") && option.DoNotRedirect)
      return add((data, option));
    invoiceId = invoiceId switch
    {
      0 when data.InvoiceId == 0 => throw new Exception("No invoice specified."),
      0 => data.InvoiceId!.Value,
      _ => invoiceId
    };

    var invoice = invoices_model.get(invoiceId);
    if (!string.IsNullOrEmpty(data.Note))
      self.input.session.set_userdata(new { payment_admin_note = data.Note });

    if (db.get_option_compare("allow_payment_amount_to_be_modified", 0))
      data.Amount = self.helper.get_invoice_total_left_to_pay(invoiceId, invoice.Total);

    data.InvoiceId = invoiceId;
    data.Invoice = invoice;
    var gateways = paymentmodes_model.get(x => x.Id == Convert.ToInt32(data.PaymentMode));
    var gateway = gateways.FirstOrDefault();
    if (gateway == null) throw new Exception("Invalid payment gateway");

    var paymentAttempt = new PaymentAttempt
    {
      Reference = uuid(),
      Amount = Convert.ToInt32(data.Amount),
      Fee = gateway.get_fee(Convert.ToInt32(data.Amount)),
      InvoiceId = data.InvoiceId,
      PaymentGateway = gateway.GetId()
    };

    option.PaymentAttempt = paymentAttemptsModel.add(paymentAttempt);
    data.Amount += paymentAttempt.Fee;
    // gateway.ProcessPayment(data);
    return 0;
  }

  public bool TransactionExists(string transactionId)
  {
    return db.InvoicePaymentRecords.Any(x => x.TransactionId == transactionId);
  }

  public bool TransactionExists(string transactionId, int invoiceId)
  {
    return db.InvoicePaymentRecords.Any(x => x.TransactionId == transactionId && x.InvoiceId == invoiceId);
  }

  public int add((InvoicePaymentRecord invoicePaymentRecord, PaymentOption option) data, bool subscription = false, bool doNotRedirect = true)
  {
    var doNotSendEmailTemplate = false;
    if (data.option.DoNotSendEmailTemplate)
    {
      doNotSendEmailTemplate = true;
    }
    else if (self.input.session.has_userdata("do_not_send_email_template"))
    {
      doNotSendEmailTemplate = true;
      self.input.session.unset_userdata("do_not_send_email_template");
    }

    if (db.is_staff_logged_in())
    {
      data.invoicePaymentRecord.Date = string.IsNullOrEmpty(data.invoicePaymentRecord.Date) ? today() : data.invoicePaymentRecord.Date;
      data.invoicePaymentRecord.Note = string.IsNullOrEmpty(data.invoicePaymentRecord.Note) ? data.invoicePaymentRecord.Note.nl2br() : self.input.session.user_data("payment_admin_note");
      // this.session.UnsetUserData("payment_admin_note");
    }
    else
    {
      data.invoicePaymentRecord.Date = today();
    }

    data.invoicePaymentRecord.DateRecorded = today();
    data = hooks.apply_filters("before_payment_recorded", data);

    var result = db.InvoicePaymentRecords.Add(data.invoicePaymentRecord);
    if (!result.IsAdded()) return 0;

    var invoice = invoices_model.get(data.invoicePaymentRecord.Invoice.Id);
    var forceUpdate = false;

    if (invoice.Status == InvoiceStatus.STATUS_DRAFT)
    {
      forceUpdate = true;
      invoices_model.change_invoice_number_when_status_draft(invoice.Id);
    }

    self.helper.update_invoice_status(data.invoicePaymentRecord.InvoiceId!.Value, forceUpdate);

    LogPaymentActivity(invoice.Id, Convert.ToDecimal(data.invoicePaymentRecord.Amount), result.Entity.Id, !db.is_staff_logged_in());
    NotifyStaffAndCustomers(data.invoicePaymentRecord, result.Entity.Id);
    hooks.do_action("after_payment_added", result.Entity.Id);
    return result.Entity.Id;
  }

  private void LogPaymentActivity(int invoiceId, decimal amount, int paymentId, bool isClient)
  {
    var invoice = invoices_model.get(invoiceId);
    var activityLangKey = isClient ? "invoice_activity_payment_made_by_client" : "invoice_activity_payment_made_by_staff";

    invoices_model.log(invoiceId, activityLangKey, isClient, JsonConvert.SerializeObject(new List<string>
    {
      db.app_format_money(amount, invoice.Currency.Name),
      $"<a href='{self.navigation.admin_url($"payments/payment/{paymentId}")}' target='_blank'>#{paymentId}</a>"
    }));

    log_activity($"Payment Recorded [ID : {paymentId}, Invoice Number: {db.format_invoice_number(invoiceId)}, Total: {db.app_format_money(amount, invoice.Currency.Name)}]");
  }

  private void NotifyStaffAndCustomers(InvoicePaymentRecord data, int paymentId)
  {
    // Implementation for sending notifications (emails, SMS, etc.)
    // Refactor complex notification logic into smaller methods for maintainability
  }

  public bool Update(InvoicePaymentRecord data)
  {
    var payment = get(data.Id);
    data.Note = data.Note.nl2br();

    data = hooks.apply_filters("before_payment_updated", data);

    var affectedRows = db.InvoicePaymentRecords.Where(x => x.Id == data.Id).Update(x => data);
    var updated = affectedRows > 0;

    if (updated && data.Amount != payment.Amount)
      self.helper.update_invoice_status(payment.InvoiceId.Value);

    hooks.do_action("after_payment_updated", new { data.Id, data, payment, updated });

    if (updated) log_activity($"Payment Updated [Number : {data.Id}]");

    return updated;
  }

  public bool Delete(int id)
  {
    var current = get(id);
    if (current == null) return false;

    hooks.do_action("before_payment_deleted", new { paymentId = id, invoiceId = current.InvoiceId });

    db.InvoicePaymentRecords.Remove(current);
    var affectedRows = db.SaveChanges();

    if (affectedRows <= 0) return false;

    self.helper.update_invoice_status(current.InvoiceId.Value);
    invoices_model.log(current.InvoiceId.Value, "invoice_activity_payment_deleted", false, JsonConvert.SerializeObject(new[]
    {
      $"{current.Id}",
      db.app_format_money(Convert.ToDecimal(current.Amount), current.Invoice.Currency.Name)
    }));

    log_activity($"Payment Deleted [ID : {id}, Invoice Number: {db.format_invoice_number(current.InvoiceId.Value)}]");

    hooks.do_action("after_payment_deleted", new { paymentId = id, invoiceId = current.InvoiceId });

    return true;
  }

  public int add_batch_payment(IEnumerable<InvoicePaymentRecord> paymentsData)
  {
    var paymentIds = new List<int>();
    foreach (var data in paymentsData)
    {
      if (string.IsNullOrEmpty(data.InvoiceId.ToString()) || data.Amount == 0 || string.IsNullOrEmpty(data.PaymentMode)) continue;

      data.DateRecorded = today();

      var result = db.InvoicePaymentRecords.Add(data);
      if (result.IsAdded()) paymentIds.Add(result.Entity.Id);
    }

    return paymentIds.Count;
  }
}
