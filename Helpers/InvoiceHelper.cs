using System.Linq.Expressions;
using Global.Entities;
using Global.Entities.Helpers;
using Service.Core.Extensions;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;
using Service.Framework.Library.Merger;
using Service.Helpers.Sale;
using Service.Models.Invoices;


namespace Service.Helpers;

public static class InvoiceHelper
{
  /**
 * Get invoice short_url
 * @since  Version 2.7.3
 * @param  object invoice
 * @return string Url
 */
  public static string get_invoice_shortlink(this HelperBase helper, Invoice invoice)
  {
    var (self, db) = getInstance();
    var long_url = helper.site_url($"invoice/{invoice.Id}/{invoice.Hash}");
    if (db.get_option_compare("bitly_access_token", null))
      return long_url;

    // Check if invoice has short link, if yes return short link
    if (!string.IsNullOrEmpty(invoice.ShortLink)) return invoice.ShortLink;

    // Create short link and return the newly created short link
    var shortLink = helper.app_generate_short_link(new
    {
      long_url,
      title = helper.format_invoice_number(invoice.Id)
    });

    if (!string.IsNullOrEmpty(shortLink)) return long_url;
    db.Invoices.Where(x => x.Id == invoice.Id).Update(x => new Invoice { ShortLink = shortLink });
    db.SaveChanges();
    return shortLink;
  }

  /**
   * Get invoice total left for paying if not payments found the original total from the invoice will be returned
   * @since  Version 1.0.1
   * @param  mixed id     invoice id
   * @param  mixed invoice_total
   * @return mixed  total left
   */
  public static string get_invoice_total_left_to_pay(this HelperBase helper, int id, decimal? invoice_total = null)
  {
    return "";
  }

  /**
   * Check if invoice email template for overdue notices is enabled
   * @return boolean
   */
  public static bool is_invoices_email_overdue_notice_enabled()
  {
    // return fapp.EmailTemplates.Any(x => x.Slug == "invoice-overdue-notice" && x.Active);
    return false;
  }

  /**
   * Check if invoice email template for due notices is enabled
   *
   * @since  2.8.0
   *
   * @return boolean
   */
  public static bool is_invoices_email_due_notice_enabled()
  {
    // return fapp.EmailTemplates.Any(x => x.Slug == "invoice-due-notice" && x.Active);
    return false;
  }

  /**
   * Check if there are sources for sending invoice overdue notices
   * Will be either email or SMS
   * @return boolean
   */
  public static string is_invoices_overdue_reminders_enabled()
  {
    return "";
  }

  /**
   * Check if there are sources for sending invoice due notices
   * Will be either email or SMS
   *
   * @since  2.8.0
   *
   * @return boolean
   */
  public static string is_invoices_due_reminders_enabled()
  {
    return "";
  }

  /**
   * Check invoice restrictions - hash, clientid
   * @since  Version 1.0.1
   * @param  mixed id   invoice id
   * @param  string hash invoice hash
   */
  public static string check_invoice_restrictions(int id, string hash)
  {
    return "";
  }

  /**
   * Format invoice status
   * @param  integer  status
   * @param  string  classes additional classes
   * @param  boolean label   To include in html label or not
   * @return mixed
   */
  public static string format_invoice_status(object status, string classes = "", bool label = true)
  {
    return "";
  }

  /**
   * Return invoice status label class baed on twitter bootstrap classses
   * @param  mixed status invoice status id
   * @return string
   */
  public static string get_invoice_status_label(object status)
  {
    return "";
  }

  /**
   * Check whether the given invoice is overdue
   *
   * @since 2.7.1
   *
   * @param  Object|array  invoice
   *
   * @return boolean
   */
  public static string is_invoice_overdue(Invoice invoice)
  {
    return "";
  }

  /**
   * Function used in invoice PDF, this public static string  will return RGBa color for PDF dcouments
   * @param  mixed status_id current invoice status id
   * @return string
   */
  public static string invoice_status_color_pdf(int status_id)
  {
    return "";
  }

  /**
   * Update invoice status
   * @param  mixed id invoice id
   * @return mixed invoice updates status / if no update return false
   * @return boolean prevent_logging do not log changes if the status is updated for the invoice activity log
   */
  public static string update_invoice_status(this HelperBase helper, int id, bool force_update = false, bool prevent_logging = false)
  {
    return "";
  }


  /**
   * Check if the invoice id is last invoice
   * @param  mixed  id invoice id
   * @return boolean
   */
  public static string is_last_invoice(int id)
  {
    return "";
  }

  /**
   * Format invoice number based on description
   * @param  mixed id
   * @return string
   */
  public static string format_invoice_number(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var invoice = db.Invoices.FirstOrDefault(x => x.Id == id);

    if (invoice == null) return "";
    var number =
      invoice.Status == InvoiceStatus.STATUS_DRAFT
        ? invoice.Prefix + "DRAFT"
        : helper.sales_number_format(invoice.Number, invoice.NumberFormat, invoice.Prefix, DateTime.Parse(invoice.Date));

    self.hooks.apply_filters("format_invoice_number", new { id, number, invoice });
    return number;
  }

  /**
   * Function that return invoice item taxes based on passed item id
   * @param  mixed itemid
   * @return array
   */
  public static List<ItemTax> get_invoice_item_taxes(this HelperBase helper, int itemid)
  {
    var (self, db) = getInstance();
    var output = db.ItemTaxes.Where(x => x.ItemId == itemid).ToList();
    return output;
  }

  /**
   * Check if payment mode is allowed for specific invoice
   * @param  mixed  id payment mode id
   * @param  mixed  invoiceid invoice id
   * @return boolean
   */
  public static bool is_payment_mode_allowed_for_invoice(int id, int invoiceid)
  {
    return false;
  }

  /**
   * Check if invoice mode exists in invoice
   * @since  Version 1.0.1
   * @param  array  modes     all invoice modes
   * @param  mixed  invoiceid invoice id
   * @param  boolean offline   should check offline or online modes
   * @return boolean
   */
  public static string found_invoice_mode(IEnumerator<object> modes, int invoiceid, bool offline = true, bool show_on_pdf = false)
  {
    return "";
  }

  /**
   * This public static string  do not work with cancelled status
   * Calculate invoices percent by status
   * @param  mixed status          estimate status
   * @param  mixed total_invoices in case the total is calculated in other place
   * @return array
   */
  public static string get_invoices_percent_by_status(object status)
  {
    return "";
  }

  /**
   * Check if staff member have assigned invoices / added as sale agent
   * @param  mixed staff_id staff id to check
   * @return boolean
   */
  public static string staff_has_assigned_invoices(int? staff_id = null)
  {
    return "";
  }

  /**
   * Load invoices total templates
   * This is the template where is showing the panels Outstanding Invoices, Paid Invoices and Past Due invoices
   * @return string
   */
  public static string load_invoices_total_template()
  {
    return "";
  }


  /**
   * Check if staff member can view invoice
   * @param  mixed id invoice id
   * @param  mixed staff_id
   * @return boolean
   */
  public static bool user_can_view_invoice(int id, int? staff_id = null)
  {
    return false;
  }

  public static async Task<Expression<Func<Invoice, bool>>> get_invoices_where_sql_for_staff(this HelperBase helper, int staff_id)
  {
    var (self, db) = getInstance();
    var has_permission_view_own = helper.has_permission("invoices", "", "view_own");
    var allow_staff_view_invoices_assigned = db.get_option("allow_staff_view_invoices_assigned");

    // Expression that will hold our condition
    Expression<Func<Invoice, bool>> whereUser;

    if (has_permission_view_own)
    {
      // Filter for invoices added by the staff member (view_own permission)
      whereUser = invoice => invoice.AddedFrom == staff_id &&
                             invoice.SaleAgentNavigation.StaffPermissions
                               .Any(sp => sp.Feature == "invoices" && sp.Capability == "view_own");

      // Allow viewing invoices where the staff member is the sale agent
      if (!string.IsNullOrEmpty(allow_staff_view_invoices_assigned))
        whereUser = whereUser.Or(invoice => invoice.SaleAgent == staff_id);
    }
    else
    {
      // Filter for invoices where the staff member is the sale agent
      whereUser = invoice => invoice.SaleAgent == staff_id;
    }

    return whereUser;
  }

  /**
   * Get invoice total left for paying if not payments found the original total from the invoice will be returned
   * @since  Version 1.0.1
   * @param  mixed $id     invoice id
   * @param  mixed $invoice_total
   * @return mixed  total left
   */
  public static double get_invoice_total_left_to_pay(this HelperBase helper, int id, double invoice_total = 0)
  {
    var (self, db) = getInstance();
    if (invoice_total == 0)
    {
      var row = db.Invoices.FirstOrDefault(x => x.Id == id);
      invoice_total = row.Total;
    }

    var payments_model = self.model.payments_model();
    var credit_notes_model = self.model.credit_notes_model();
    var payments = payments_model.get_invoice_payments(id);
    var credits = credit_notes_model.get_applied_invoice_credits(id);
    payments = (List<InvoicePaymentRecord>)TypeMerger.Merge(payments, credits);
    var totalPayments = payments.Select(payment => Convert.ToDouble(payment.Amount)).ToList().Sum();
    // return helper.number_format(invoice_total - totalPayments, get_decimal_places());
    return invoice_total - totalPayments;
  }
}
