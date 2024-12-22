using Global.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Tags;

namespace Service.Controllers;

public class InvoiceController(ILogger<InvoiceController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index_get(int id, string hash)
  {
    var (self, db) = getInstance();
    var payments_model = self.model.payments_model();
    var invoices_model = self.model.invoices_model();
    var payment_modes_model = self.model.payment_modes_model();
    check_invoice_restrictions(id, hash);
    var invoice = invoices_model.get(id);

    invoice = self.hooks.apply_filters("before_client_view_invoice", invoice);

    if (!is_client_logged_in())
      self.helper.load_client_language(invoice.ClientId);

    // this.app_scripts.theme('sticky-js', 'assets/plugins/sticky/sticky.js');
    // self.library.app_number_to_word(new { ClientId = invoice.ClientId }, "numberword");

    data.payments = payments_model.get_invoice_payments(id);
    data.payment_modes = payment_modes_model.get();
    data.title = self.helper.format_invoice_number(invoice.Id);
    // this.disableNavigation();
    // this.disableSubMenu();
    data.hash = hash;
    data.invoice = self.hooks.apply_filters("invoice_html_pdf_data", invoice);
    data.bodyclass = "viewinvoice";
    // data(data);
    // this.view('invoicehtml');
    // add_views_tracking('invoice', id);
    // self.hooks.do_action('invoice_html_viewed', id);
    // no_index_customers_area();
    // this.layout();
    return Ok();
  }

  [HttpGet]
  public IActionResult index(int id, string hash)
  {
    var (self, db) = getInstance();
    var invoices_model = self.model.invoices_model();
    var payments_model = self.model.payments_model();
    var payment_modes_model = self.model.payment_modes_model();

    check_invoice_restrictions(id, hash);
    var invoice = invoices_model.get(id);

    invoice = self.hooks.apply_filters("before_client_view_invoice", invoice);

    if (!is_client_logged_in())
      self.helper.load_client_language(invoice.ClientId);

    // Handle Invoice PDF generator
    if (self.input.post().ContainsKey("invoicepdf"))
    {
      PdfDocumentGenerator pdf;
      try
      {
        pdf = self.helper.invoice_pdf(invoice);
      }
      catch (Exception e)
      {
        return MakeError(e.Message);
      }

      var invoice_number = self.helper.format_invoice_number(invoice.Id);
      var companyname = db.get_option("invoice_company_name");
      if (!string.IsNullOrEmpty(companyname)) invoice_number += "-" + slug_it(companyname).ToUpper();
      pdf.Output(slug_it(invoice_number).ToUpper() + ".pdf");
      return Ok();
    }

    // Handle _POST payment
    if (self.input.post().ContainsKey("make_payment"))
    {
      if (!self.input.post().ContainsKey("paymentmode"))
      {
        set_alert("warning", self.helper.label("invoice_html_payment_modes_not_selected"));
        return Redirect(self.helper.site_url("invoice/" + id + "/" + hash));
      }

      if ((self.input.post().ContainsKey("amount") || self.input.post<int>("amount") == 0)
          && db.get_option_compare("allow_payment_amount_to_be_modified", true)
         )
      {
        set_alert("warning", self.helper.label("invoice_html_amount_blank"));
        return Redirect(self.helper.site_url("invoice/" + id + "/" + hash));
      }

      payments_model.process_payment(self.input.post<InvoicePaymentRecord>(), id);
    }

    if (self.input.post().ContainsKey("paymentpdf"))
    {
      id = self.input.post<int>("paymentpdf");
      var payment = payments_model.get(id);
      payment.Invoice = invoices_model.get(payment.Invoice.Id);
      var paymentpdf = self.helper.payment_pdf(payment);
      paymentpdf.Output(slug_it(self.helper.label("payment") + "-" + payment.Id).ToUpper() + ".pdf");
      return Ok();
    }

    // this.app_scripts.theme('sticky-js', 'assets/plugins/sticky/sticky.js');
    // var app_number_to_word = self.library.app_number_to_word(new { ClientId = invoice.ClientId });
    data.payments = payments_model.get_invoice_payments(id);
    data.payment_modes = payment_modes_model.get();
    data.title = self.helper.format_invoice_number(invoice.Id);
    // this.disableNavigation();
    // this.disableSubMenu();
    data.hash = hash;
    data.invoice = self.hooks.apply_filters("invoice_html_pdf_data", invoice);
    data.bodyclass = "viewinvoice";
    data(data);
    // this.view('invoicehtml');
    // add_views_tracking('invoice', id);
    self.hooks.do_action("invoice_html_viewed", id);
    // no_index_customers_area();
    // this.layout();
    return Ok();
  }
}
