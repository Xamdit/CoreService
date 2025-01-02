using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Framework.Library.Merger;
using Service.Helpers;
using Service.Helpers.Pdf;
using Service.Helpers.Tags;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractController(ILogger<ContractController> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  [HttpGet]
  public IActionResult Index([FromQuery] int id, [FromQuery] string hash)
  {
    var contractsModel = self.contracts_model(db);
    self.helper.check_contract_restrictions(id, hash);
    var rows = contractsModel.get(x => x.Id == id);
    if (rows.Count == 0)
      return NotFound();
    var row = rows.First();

    if (!db.is_client_logged_in())
      self.helper.load_client_language(row.contract.Client);

    // DisableNavigation();
    // DisableSubMenu();

    var data = new
    {
      title = row.contract.Subject,
      contract = hooks.apply_filters("contract_html_pdf_data", row.contract),
      bodyclass = "contract contract-view identity-confirmation",
      identity_confirmation_enabled = true,
      comments = contractsModel.get_comments(id)
    };

    // self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    // self.app_css.remove("reset-css", "customers-area-default");
    data = hooks.apply_filters("contract_customers_area_view_data", data);

    // NoIndexCustomersArea();
    // view("contracthtml", data);
    // return Layout();
    return Ok();
  }

  [HttpPost]
  public IActionResult Index([FromForm] int id, [FromForm] string hash, [FromForm] string action, [FromForm] string signature, [FromForm] string content)
  {
    var contractsModel = self.contracts_model(db);
    self.helper.check_contract_restrictions(id, hash);
    var contract = contractsModel.get(x => x.Id == id);
    if (contract.Count == 0)
      return NotFound();

    var row = contract.First();

    if (!db.is_client_logged_in())
      self.helper.load_client_language(row.contract.Client);

    switch (action)
    {
      case "contract_pdf":
        var pdf = self.helper.contract_pdf(contract);
        pdf.Output(slug_it(row.contract.Subject + "-" + db.get_option("companyname")) + ".pdf");
        break;
      case "sign_contract":
        process_digital_signature_image(self.input.post("signature"), CONTRACTS_UPLOADS_FOLDER + id);
        var dataset = TypeMerger.Merge(get_acceptance_info_array<Contract>(), new Contract() { Signed = true });
        db.Contracts.Where(x => x.Id == id).Update(x => dataset);
        self.helper.send_contract_signed_notification_to_staff(id);
        set_alert("success", "Document signed successfully");
        return Redirect(Request.Headers["Referer"]);

      case "contract_comment":
        if (string.IsNullOrEmpty(content))
          return Redirect(Request.Path);
        var contracts_model = self.contracts_model(db);
        var contract_data = self.input.post<ContractComment>();
        contract_data.ContractId = id;
        contracts_model.add_comment(contract_data, true);
        return Redirect(base_url() + "?tab=discussion");
    }

    // DisableNavigation();
    // DisableSubMenu();

    var data = new
    {
      title = row.contract.Subject,
      contract = hooks.apply_filters("contract_html_pdf_data", contract),
      bodyclass = "contract contract-view identity-confirmation",
      identity_confirmation_enabled = true,
      comments = contractsModel.get_comments(id)
    };

    // self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    // self.app_css.remove("reset-css", "customers-area-default");
    data = hooks.apply_filters("contract_customers_area_view_data", data);

    // NoIndexCustomersArea();
    // View("contracthtml", data);
    // return Layout();
    return Ok();
  }
}
