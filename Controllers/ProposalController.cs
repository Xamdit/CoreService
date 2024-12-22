using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Proposals;
using Service.Helpers.Tags;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalController(ILogger<ProposalController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index_get(int id, string hash)
  {
    var (self, db) = getInstance();
    var proposals_model = self.model.proposals_model();
    self.helper.check_proposal_restrictions(id, hash);
    var proposal = proposals_model.get(x => x.Id == id);

    if (proposal.RelType == "customer" && !is_client_logged_in())
      self.helper.load_client_language(proposal.RelId);
    else if (proposal.RelType == "lead")
      self.helper.load_lead_language(proposal.RelId);
    var identity_confirmation_enabled = db.get_option("proposal_accept_identity_confirmation");
    // var number_word_lang_rel_id = "unknown";
    var number_word_lang_rel_id = 0;
    if (proposal.RelType == "customer")
      number_word_lang_rel_id = proposal.RelId;
    // self.library.load("app_number_to_word", new
    // {
    //   clientid = number_word_lang_rel_id
    // }, "numberword");

    // this.disableNavigation();
    // this.disableSubMenu();
    data.title = proposal.Subject;
    data.proposal = self.hooks.apply_filters("proposal_html_pdf_data", proposal);
    data.bodyclass = "proposal proposal-view";
    data.identity_confirmation_enabled = identity_confirmation_enabled;
    if (identity_confirmation_enabled == "1") data.bodyclass += " identity-confirmation";

    self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    data.comments = proposals_model.get_comments(id);
    self.helper.add_views_tracking("proposal", id);
    self.hooks.do_action("proposal_html_viewed", id);
    this.app_css.remove("reset-css", "customers-area-default");
    data = self.hooks.apply_filters("proposal_customers_area_view_data", data);
    self.helper.no_index_customers_area();
    // data(data);
    // this.view("viewproposal");
    // this.layout();
    return Ok();
  }

  [HttpPost]
  public IActionResult index(int id, string hash)
  {
    var (self, db) = getInstance();
    var proposals_model = self.model.proposals_model();
    self.helper.check_proposal_restrictions(id, hash);
    var proposal = proposals_model.get(x => x.Id == id);

    if (proposal.RelType == "customer" && !is_client_logged_in())
      self.helper.load_client_language(proposal.RelId);
    else if (proposal.RelType == "lead")
      self.helper.load_lead_language(proposal.RelId);

    var identity_confirmation_enabled = db.get_option("proposal_accept_identity_confirmation");

    var action = self.input.post("action");
    switch (
      action)
    {
      case "proposal_pdf":
        var proposal_number = self.helper.format_proposal_number(id);
        var companyname = db.get_option("invoice_company_name");
        if (companyname != "") proposal_number += "-" + self.helper.mb_strtoupper(slug_it(companyname), "UTF-8");
        PdfDocumentGenerator pdf;
        try
        {
          pdf = self.library.proposal_pdf(proposal);
        }
        catch (Exception e)
        {
          return MakeError(e.Message);
        }

        pdf.Output(proposal_number + ".pdf");

        break;
      case "proposal_comment":
        // comment is blank
        if (string.IsNullOrEmpty(self.input.post("content")))
          return Redirect(self.helper.site_url());
        data = self.input.post();
        data.proposalid = id;
        proposals_model.add_comment(data, true);
        return Redirect(self.helper.site_url() + "?tab=discussion");
        break;
      case "accept_proposal":
        var success = proposals_model.mark_action_status(3, id, true);
        if (success)
        {
          self.helper.process_digital_signature_image(self.input.post("signature"), PROPOSAL_ATTACHMENTS_FOLDER + id);
          db.Proposals.Where(x => x.Id == id).Update(x => get_acceptance_info_array());
          return Redirect(self.helper.site_url("refresh"));
        }

        break;
      case "decline_proposal":
        success = proposals_model.mark_action_status(2, id, true);
        if (success)
          return Redirect(self.helper.base_url("refresh"));
        break;
    }

    return Ok();
  }
}
