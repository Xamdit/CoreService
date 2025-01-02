using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Library.Merger;
using Service.Helpers.Pdf;
using Service.Models.Client;
using Service.Models.CreditNotes;
using Service.Models.Invoices;

namespace Service.Models.Statements;

public class StatementModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  private ClientsModel clients_model = self.clients_model(db);
  private CurrenciesModel currencies_model = self.currencies_model(db);

  public async Task<StatementResult> get_statement(int customerId, DateTime from, DateTime to)
  {
    var invoices = db.Invoices
      .Where(i => i.ClientId == customerId && i.Status != InvoiceStatus.STATUS_DRAFT && i.Status != InvoiceStatus.STATUS_CANCELLED)
      .Where(i => from == to ? DateTime.Parse(i.Date) == from : DateTime.Parse(i.Date) >= from && DateTime.Parse(i.Date) <= to)
      .Select(i => new CreditNoteRefundStatement
      {
        InvoiceId = i.Id,
        Date = i.Date,
        DueDate = i.DueDate,
        Total = i.Total,
        TmpDate = i.Date + " " + i.DateCreated.ToString("HH:mm:ss")
      })
      .OrderByDescending(i => i.Date)
      .ToList();

    var creditNotes = db.CreditNotes
      .Where(cn => cn.ClientId == customerId && cn.Status != CreditNoteStatus.Cancelled)
      .Where(cn => from == to ? cn.Date == from : cn.Date >= from && cn.Date <= to)
      .Select(cn => new CreditNoteRefundStatement
      {
        CreditNoteId = cn.Id,
        Date = cn.Date.ToString("Y-m-d H:i:s"),
        TmpDate = cn.Date + " " + cn.DateCreated.ToString("HH:mm:ss"),
        Total = cn.Total
      })
      .ToList();

    var creditsApplied = db.Credits
      // .Where(c => c.Staff.ClientId == customerId)
      .Where(c => from == to ? c.Date == from : c.Date >= from && c.Date <= to)
      .Select(c => new CreditNoteRefundStatement
      {
        CreditId = c.Id,
        InvoiceId = c.InvoiceId,
        CreditAppliedCreditNoteId = c.Id,
        Date = c.Date.ToString("Y-m-d H:i:s"),
        TmpDate = c.Date + " " + c.DateApplied.ToString("HH:mm:ss"),
        Amount = c.Amount
      })
      .ToList();

    var payments = db.InvoicePaymentRecords
      .Where(p => p.Invoice.ClientId == customerId)
      .Where(p => from == to ? DateTime.Parse(p.Date) == from : DateTime.Parse(p.Date) >= from && DateTime.Parse(p.Date) <= to)
      .Select(p => new CreditNoteRefundStatement
      {
        PaymentId = p.Id,
        Date = p.Date,
        // TmpDate = p.Date + " " + p.DateRecorded.ToString("HH:mm:ss"),
        TmpDate = p.Date + " " + today(),
        PaymentInvoiceId = p.InvoiceId,
        Amount = Convert.ToInt32(p.Amount)
      })
      .ToList();

    var creditNoteRefunds = db.CreditNoteRefunds
      .Where(cr => cr.CreditNote.ClientId == customerId)
      .Where(cr => from == to ? DateTime.Parse(cr.RefundedOn) == from : DateTime.Parse(cr.RefundedOn) >= from && DateTime.Parse(cr.RefundedOn) <= to)
      .Select(cr => new CreditNoteRefundStatement
      {
        CreditNoteRefundId = cr.Id,
        CreditNoteId = cr.CreditNoteId,
        Amount = cr.Amount,
        TmpDate = cr.RefundedOn + " " + cr.CreatedAt,
        Date = cr.RefundedOn
      })
      .ToList();

    var mergedResults = (List<CreditNoteRefundStatement>)TypeMerger.Merge(invoices, payments);
    mergedResults = (List<CreditNoteRefundStatement>)TypeMerger.Merge(mergedResults, creditNotes);
    mergedResults = (List<CreditNoteRefundStatement>)TypeMerger.Merge(mergedResults, creditsApplied);
    mergedResults = (List<CreditNoteRefundStatement>)TypeMerger.Merge(mergedResults, creditNoteRefunds);
    mergedResults = mergedResults.OrderBy(r => r.TmpDate).ToList();

    var invoicedAmount = db.Invoices
      .Where(i => i.ClientId == customerId && i.Status != InvoiceStatus.STATUS_DRAFT && i.Status != InvoiceStatus.STATUS_CANCELLED)
      .Where(i => DateTime.Parse(i.Date) >= from && DateTime.Parse(i.Date) <= to)
      .Sum(i => i.Total);

    var creditNotesAmount = db.CreditNotes
      .Where(cn => cn.ClientId == customerId && cn.Status != CreditNoteStatus.Cancelled)
      .Where(cn => cn.Date >= from && cn.Date <= to)
      .Sum(cn => cn.Total);

    var refundsAmount = db.CreditNoteRefunds
      .Where(cr => cr.CreditNote.ClientId == customerId && DateTime.Parse(cr.RefundedOn) >= from && DateTime.Parse(cr.RefundedOn) <= to)
      .Sum(cr => cr.Amount);

    invoicedAmount -= creditNotesAmount;

    var amountPaid = db.InvoicePaymentRecords
      .Where(p => p.Invoice.ClientId == customerId && DateTime.Parse(p.Date) >= from && DateTime.Parse(p.Date) <= to)
      .ToList()
      .Select(x => x.Amount)
      .Sum();

    var beginningBalance = await CalculateBeginningBalance(customerId, from);

    amountPaid -= refundsAmount;

    var balanceDue = invoicedAmount - amountPaid + beginningBalance + refundsAmount;

    var result = new StatementResult
    {
      MergedResults = mergedResults,
      InvoicedAmount = invoicedAmount,
      CreditNotesAmount = creditNotesAmount,
      RefundsAmount = refundsAmount,
      AmountPaid = amountPaid,
      BeginningBalance = beginningBalance,
      BalanceDue = balanceDue,
      ClientId = customerId,
      Client = clients_model.get(x => x.Id == customerId).First(),
      From = from,
      To = to,
      Currency = await get_customer_currency(customerId)
    };

    return result;
  }

  private async Task<double> CalculateBeginningBalance(int customerId, DateTime from)
  {
    var invoicesBefore = db.Invoices
      .Where(i => i.ClientId == customerId && DateTime.Parse(i.Date) < from && i.Status != InvoiceStatus.STATUS_DRAFT && i.Status != InvoiceStatus.STATUS_CANCELLED)
      .Sum(i => i.Total);

    var paymentsBefore = db.InvoicePaymentRecords
      .Where(p => p.Invoice.ClientId == customerId && DateTime.Parse(p.Date) < from)
      .Sum(p => p.Amount);

    var creditNotesBefore = db.CreditNotes
      .Where(cn => cn.ClientId == customerId && cn.Date < from && cn.Status != CreditNoteStatus.Cancelled)
      .Sum(cn => cn.Total);

    var refundsBefore = db.CreditNoteRefunds
      .Where(cr => cr.CreditNote.ClientId == customerId && DateTime.Parse(cr.RefundedOn) < from)
      .Sum(cr => cr.Amount);

    return invoicesBefore - paymentsBefore - creditNotesBefore + refundsBefore;
  }

  private async Task<Currency> get_customer_currency(int customerId)
  {
    var customerCurrencyId = clients_model.get_customer_default_currency(customerId);
    return customerCurrencyId != 0
      ? currencies_model.get(customerCurrencyId)
      : currencies_model.get_base_currency();
  }


  /// <summary>
  /// Send customer statement to email.
  /// </summary>
  /// <param name="customerId">Customer ID.</param>
  /// <param name="sendTo">List of contact emails to send.</param>
  /// <param name="from">Date from.</param>
  /// <param name="to">Date to.</param>
  /// <param name="cc">Email CC.</param>
  /// <returns>Boolean indicating if the email was sent successfully.</returns>
  public async Task<bool> send_statement_to_email(int customerId, List<int> sendTo, string from, string to, string cc = "")
  {
    var sent = false;

    if (sendTo is not { Count: > 0 }) return false;
    var statement = await get_statement(customerId, DateTime.Parse(from), DateTime.Parse(to));
    self.helper.set_mailing_constant();
    var pdf = self.helper.statement_pdf(statement);
    var pdfFileName = slug_it($"customer_statement-{statement.Client.Company}");
    var attach = pdf.Output($"{pdfFileName}.pdf");

    var i = 0;
    foreach (var contactId in sendTo)
    {
      if (contactId != 0)
      {
        // Send cc only for the first contact
        if (!string.IsNullOrEmpty(cc) && i > 0) cc = "";

        var contact = clients_model.get_contact(contactId);

        var template = mail_template("customer_statement", contact.Email, contactId, statement, cc);
        // var attachment = new
        // {
        //   attachment = attach,
        //   filename = pdfFileName + ".pdf",
        //   type = "application/pdf"
        // };
        // template.add_attachment(attachment);

        if (template.send()) sent = true;
      }

      i++;
    }

    return sent;
  }
}
