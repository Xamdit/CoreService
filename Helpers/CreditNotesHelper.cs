using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Models.CreditNotes;
using Service.Models.Invoices;

namespace Service.Helpers;

public static class CreditNoteHelper
{
  // Sum total credits applied for invoice
  public static decimal? total_credits_applied_to_invoice(this HelperBase helper, int invoiceId)
  {
    var total = helper.sum_from_table("credits", new { field = "amount", where = new { invoice_id = invoiceId } });

    return total == 0 ? null : total;
  }

  // Return credit note status color RGBA for pdf
  public static string credit_note_status_color_pdf(this HelperBase helper, int statusId)
  {
    return statusId switch
    {
      1 => "3, 169, 244", // Status 1
      2 => "132, 197, 41", // Status 2
      _ => "119, 119, 119" // Status VOID
    };
  }

  // Return array with invoices IDs statuses which can be applied credits
  public static List<int> invoices_statuses_available_for_credits(this CreditNotesModel model)
  {
    var (self, db) = model.getInstance();
    return hooks.apply_filters("invoices_statuses_available_for_credits", new List<int>
    {
      InvoiceStatus.STATUS_UNPAID,
      InvoiceStatus.STATUS_PARTIALLY,
      InvoiceStatus.STATUS_DRAFT,
      InvoiceStatus.STATUS_OVERDUE
    });
  }

  // Check if credits can be applied to invoice based on the invoice status
  public static bool credits_can_be_applied_to_invoice(this CreditNotesModel model, int statusId)
  {
    return model.invoices_statuses_available_for_credits().Contains(statusId);
  }

  // Check if it is the last credit note created
  public static bool is_last_credit_note(this CreditNotesModel model, int id)
  {
    var (self, db) = model.getInstance();
    var row = db.CreditNotes.LastOrDefault(x => x.Id == id);
    return row.Id == id;
  }

  // Function that formats credit note number based on the prefix option and the credit note number
  public static string format_credit_note_number(this CreditNotesModel model, int id)
  {
    var (_, db) = model.getInstance();
    var creditNote = db.CreditNotes.FirstOrDefault(x => x.Id == id);
    if (creditNote == null) return string.Empty;
    // var number = sales_number_format(creditNote.Number, creditNote.NumberFormat, creditNote.Prefix, creditNote.Date);
    var number = db.sales_number_format(creditNote.Number, creditNote.NumberFormat, creditNote.Prefix, creditNote.Date);

    hooks.apply_filters("format_credit_note_number", new { id, number, creditNote });
    return number;
  }

  // Format credit note status
  public static string format_credit_note_status(this CreditNotesModel model, int status, bool text = false)
  {
    var (self, db) = model.getInstance();
    var credit_notes_model = self.credit_notes_model(db);

    var statuses = credit_notes_model.get_statuses();
    var statusArray = statuses.FirstOrDefault(s => s.id == status);

    if (statusArray == null) return status.ToString();

    if (text) return statusArray.name;

    var style = $"border: 1px solid {statusArray.color}; color: {statusArray.color};";
    var className = "label s-status";

    return $"<span class=\"{className}\" style=\"{style}\">{statusArray.name}</span>";
  }

  // Function that returns credit note item taxes based on passed item id
  public static List<ItemTax> GetCreditNoteItemTaxes(this MyContext db, int itemId)
  {
    var taxes = db.ItemTaxes
      .Where(x => x.ItemId == itemId && x.RelType == "credit_note")
      .ToList()
      .Select(x =>
      {
        x.TaxName = $"{x.TaxName}|{x.TaxRate}";
        return x;
      }).ToList();

    return taxes;
  }

  // Helper methods (placeholders for actual implementations)
  private static decimal sum_from_table(this HelperBase helper, string tableName, object conditions)
  {
    // Placeholder for summing logic from a table
    return 0;
  }


  // public static string sales_number_format(this HelperBase helper, int number, string numberFormat, string prefix, DateTime date)
  // {
  //   // Placeholder for formatting sales numbers
  //   return $"{prefix}-{number}";
  // }

  /**
 * Function that return credit note item taxes based on passed item id
 * @param  mixed $itemid
 * @return array
 */
  public static List<ItemTax> get_credit_note_item_taxes(this MyContext db, int itemid)
  {
    var taxes = db.ItemTaxes
      .Where(x => x.ItemId == itemid && x.RelType == "credit_note")
      .ToList()
      .Select(tax =>
      {
        tax.TaxName = tax.TaxName + "|" + tax.TaxRate;
        return tax;
      })
      .ToList();
    return taxes;
  }
}
