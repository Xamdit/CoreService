using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Helpers.Tags;

namespace Service.Controllers;

public class InvoiceController(ILogger<FormsController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index_get(int id, string hash)
  {
    var (self, db) = getInstance();
    var invoices_model = self.model.invoices_model();
    check_invoice_restrictions(id, hash);
    var invoice = invoices_model.get(id);

    invoice = self.hooks.apply_filters("before_client_view_invoice", invoice);

    if (!is_client_logged_in())
      self.helper.load_client_language(invoice.ClientId);

    this.app_scripts.theme('sticky-js', 'assets/plugins/sticky/sticky.js');
    this.load.library('app_number_to_word', [
      'clientid' => invoice.clientid,
      ],'numberword');
    this.load.model('payment_modes_model');
    this.load.model('payments_model');
    data.payments = this.payments_model.get_invoice_payments(id);
    data.payment_modes = this.payment_modes_model.get();
    data.title = format_invoice_number(invoice.id);
    this.disableNavigation();
    this.disableSubMenu();
    data.hash = hash;
    data.invoice = self.hooks.apply_filters('invoice_html_pdf_data', invoice);
    data.bodyclass = 'viewinvoice';
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
    check_invoice_restrictions(id, hash);
    var invoice = invoices_model.get(id);

    invoice = self.hooks.apply_filters("before_client_view_invoice", invoice);

    if (!is_client_logged_in())
      self.helper.load_client_language(invoice.ClientId);

    // Handle Invoice PDF generator
    if (this.input.post('invoicepdf'))
    {
      try
      {
        pdf = invoice_pdf(invoice);
      }
      catch (Exception
             e)
      {
        echo e.getMessage();
        die;
      }

      invoice_number = format_invoice_number(invoice.id);
      companyname = get_option('invoice_company_name');
      if (companyname != '')
      {
        invoice_number.= '-'.mb_strtoupper(slug_it(companyname), 'UTF-8');
      }

      pdf.Output(mb_strtoupper(slug_it(invoice_number), 'UTF-8'). '.pdf', 'D');
      die();
    }

    // Handle _POST payment
    if (this.input.post('make_payment'))
    {
      this.load.model('payments_model');
      if (!this.input.post('paymentmode'))
      {
        set_alert('warning', _l('invoice_html_payment_modes_not_selected'));
        redirect(site_url('invoice/'.id. '/'.hash));
      }

      elseif((!this.input.post('amount') || this.input.post('amount') == 0) && get_option('allow_payment_amount_to_be_modified') == 1) {
        set_alert('warning', _l('invoice_html_amount_blank'));
        redirect(site_url('invoice/'.id. '/'.hash));
      }
      this.payments_model.process_payment(this.input.post(), id);
    }

    if (this.input.post('paymentpdf'))
    {
      id = this.input.post('paymentpdf');
      payment = this.payments_model.get(id);
      payment.invoice_data = invoices_model.get(payment.invoiceid);
      paymentpdf = payment_pdf(payment);
      paymentpdf.Output(mb_strtoupper(slug_it(_l('payment'). '-'.payment.paymentid), 'UTF-8'). '.pdf', 'D');
      die;
    }

    this.app_scripts.theme('sticky-js', 'assets/plugins/sticky/sticky.js');
    this.load.library('app_number_to_word', [
      'clientid' => invoice.clientid,
      ],'numberword');
    this.load.model('payment_modes_model');
    this.load.model('payments_model');
    data.payments = this.payments_model.get_invoice_payments(id);
    data.payment_modes = this.payment_modes_model.get();
    data.title = format_invoice_number(invoice.id);
    this.disableNavigation();
    this.disableSubMenu();
    data.hash = hash;
    data.invoice = self.hooks.apply_filters("invoice_html_pdf_data", invoice);
    data.bodyclass = 'viewinvoice';
    data(data);
    this.view('invoicehtml');
    add_views_tracking('invoice', id);
    self.hooks.do_action('invoice_html_viewed', id);
    no_index_customers_area();
    // this.layout();
    return Ok();
  }
}
