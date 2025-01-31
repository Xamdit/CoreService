using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Framework.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Proposals;
using Service.Helpers.Tags;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalController(ILogger<ClientControllerBase> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  [HttpGet]
  public IActionResult index_get(int id, string hash)
  {
    var proposals_model = self.proposals_model(db);
    self.helper.check_proposal_restrictions(id, hash);
    var proposal = proposals_model.get(x => x.Id == id);

    if (proposal.RelType == "customer" && !db.is_client_logged_in())
      self.helper.load_client_language(proposal.RelId);
    else if (proposal.RelType == "lead")
      db.load_lead_language(proposal.RelId);
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
    data.proposal = hooks.apply_filters("proposal_html_pdf_data", proposal);
    data.bodyclass = "proposal proposal-view";
    data.identity_confirmation_enabled = identity_confirmation_enabled;
    if (identity_confirmation_enabled == "1") data.bodyclass += " identity-confirmation";

    // self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    data.comments = proposals_model.get_comments(id);
    // self.helper.add_views_tracking("proposal", id);
    hooks.do_action("proposal_html_viewed", id);
    // self.app_css.remove("reset-css", "customers-area-default");
    data = hooks.apply_filters("proposal_customers_area_view_data", data);
    // self.helper.no_index_customers_area();
    // data(data);
    // this.view("viewproposal");
    // this.layout();
    return Ok();
  }

  [HttpPost]
  public IActionResult index(int id, string hash)
  {
    var proposals_model = self.proposals_model(db);
    self.helper.check_proposal_restrictions(id, hash);
    var proposal = proposals_model.get(x => x.Id == id);

    if (proposal.RelType == "customer" && !db.is_client_logged_in())
      self.helper.load_client_language(proposal.RelId);
    else if (proposal.RelType == "lead")
      db.load_lead_language(proposal.RelId);

    var identity_confirmation_enabled = db.get_option("proposal_accept_identity_confirmation");

    var action = self.input.post("action");
    switch (
      action)
    {
      case "proposal_pdf":
        var proposal_number = db.format_proposal_number(id);
        var companyname = db.get_option("invoice_company_name");
        if (companyname != "")
          proposal_number += "-" + db.slug_it(companyname).ToUpper();
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
          return Redirect(site_url());
        data = self.input.post<dynamic>();
        data.proposalid = id;
        proposals_model.add_comment(data, true);
        return Redirect(site_url() + "?tab=discussion");
        break;
      case "accept_proposal":
        var success = proposals_model.mark_action_status(3, id, true);
        if (success)
        {
          process_digital_signature_image(self.input.post("signature"), PROPOSAL_ATTACHMENTS_FOLDER + id);
          db.Proposals
            .Where(x => x.Id == id)
            .Update(x => get_acceptance_info_array<Proposal>(false));
          return Redirect(self.helper.site_url("refresh"));
        }

        break;
      case "decline_proposal":
        success = proposals_model.mark_action_status(2, id, true);
        if (success)
          return Redirect(base_url("refresh"));
        break;
    }

    return Ok();
  }
}
