using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Core.InputSet;
using Service.Framework.Helpers;
using Service.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Tags;
using Service.Libraries.AppNumberToWords;

namespace Service.Controllers;

public class EstimateController(ILogger<EstimateController> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  [HttpGet]
  public IActionResult index(int id, string hash, string signature, string estimate_action)
  {
    var estimates_model = self.estimates_model(db);
    var invoices_model = self.invoices_model(db);
    // self.helper.check_estimate_restrictions(id, hash);
    var estimate = estimates_model.get(x => x.Id == id).First();
    if (!db.is_client_logged_in())
      self.helper.load_client_language(estimate.ClientId.Value);

    var identity_confirmation_enabled = db.get_option("estimate_accept_identity_confirmation");
    var redURL = "#";
    if (self.input.post_has("estimate_action"))
    {
      var action = self.input.post<int>("estimate_action");
      // Only decline and accept allowed
      if (action is not (3 or 4)) return Redirect(redURL);
      var success = estimates_model.mark_action_status(action, id, true);
      redURL = base_url();
      var accepted = false;
      if (self.helper.is_array(success) && success.invoice != null)
      {
        accepted = true;
        var _invoice = invoices_model.get(success.invoice.Id);
        set_alert("success", self.helper.label("clients_estimate_invoiced_successfully"));
        redURL = self.helper.site_url("invoice/" + _invoice.Id + "/" + _invoice.Hash);
      }
      else if ((self.helper.is_array(success) && success.invoice == null) || success.is_success)
      {
        if (action == 4)
        {
          accepted = true;
          set_alert("success", self.helper.label("clients_estimate_accepted_not_invoiced"));
        }
        else
        {
          set_alert("success", self.helper.label("clients_estimate_declined"));
        }
      }
      else
      {
        set_alert("warning", self.helper.label("clients_estimate_failed_action"));
      }

      if (!(action == 4 && accepted)) return Redirect(redURL);
      self.helper.process_digital_signature_image(signature, ESTIMATE_ATTACHMENTS_FOLDER + id);
      db.Estimates.Where(x => x.Id == id).Update(x => get_acceptance_info_array<Estimate>());

      return Redirect(redURL);
    }

    // Handle Estimate PDF generator
    if (self.input.post_has("estimatepdf"))
    {
      PdfDocumentGenerator pdf;
      try
      {
        pdf = self.library.estimate_pdf(estimate);
      }
      catch (Exception e)
      {
        return MakeError(e.Message);
      }

      var estimate_number = db.format_estimate_number(estimate.Id);
      var companyname = db.get_option("invoice_company_name");
      if (companyname != "") estimate_number += "-" + db.slug_it(companyname).ToUpper();
      pdf.Output(db.slug_it(estimate_number).ToUpper() + ".pdf");
      return MakeError();
    }

    self.library.app_number_to_word(new Estimate() { ClientId = estimate.ClientId }, "numberword");
    //self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");

    data.title = db.format_estimate_number(estimate.Id);
    // this.disableNavigation();
    // this.disableSubMenu();
    data.hash = hash;
    data.can_be_accepted = false;
    data.estimate = hooks.apply_filters("estimate_html_pdf_data", estimate);
    data.bodyclass = "viewestimate";
    data.Identity_confirmation_enabled = identity_confirmation_enabled;
    if (identity_confirmation_enabled == "1") data.bodyclass += " identity-confirmation";

    // this.view("estimatehtml");
    // add_views_tracking("estimate", id);
    // hooks.do_action("estimate_html_viewed", id);
    // no_index_customers_area();
    // this.layout();
    return this.MakeSuccess(data);
  }
}
