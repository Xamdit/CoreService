using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;
using Service.Helpers;
using Service.Helpers.Tags;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractController(ILogger<ContractController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult Index([FromQuery] int id, [FromQuery] string hash)
  {
    var (self, db) = getInstance();
    var contractsModel = self.model.contracts_model();
    self.helper.check_contract_restrictions(id, hash);
    var rows = contractsModel.get(x => x.Id == id);
    if (rows.Count == 0)
      return NotFound();
    var row = rows.First();

    if (!self.helper.is_client_logged_in())
      self.helper.load_client_language(row.contract.Client);

    // DisableNavigation();
    // DisableSubMenu();

    var data = new
    {
      title = row.contract.Subject,
      contract = self.hooks.apply_filters("contract_html_pdf_data", row.contract),
      bodyclass = "contract contract-view identity-confirmation",
      identity_confirmation_enabled = true,
      comments = contractsModel.get_comments(id)
    };

    // self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    // self.app_css.remove("reset-css", "customers-area-default");
    data = self.hooks.apply_filters("contract_customers_area_view_data", data);

    // NoIndexCustomersArea();
    // view("contracthtml", data);
    // return Layout();
    return Ok();
  }

  [HttpPost]
  public IActionResult Index([FromForm] int id, [FromForm] string hash, [FromForm] string action, [FromForm] string signature, [FromForm] string content)
  {
    var (self, db) = getInstance();
    var contractsModel = self.model.contracts_model();

    CheckContractRestrictions(id, hash);
    var contract = contractsModel.get(x => x.Id == id);
    if (contract.Count == 0)
      return NotFound();

    var row = contract.First();

    if (!IsClientLoggedIn())
      self.helper.load_client_language(row.contract.Client);

    switch (action)
    {
      case "contract_pdf":
        var pdf = GenerateContractPdf(contract);
        return File(pdf, "application/pdf", Slugify(row.contract.Subject) + ".pdf");

      case "sign_contract":
        ProcessDigitalSignature(signature, id);
        UpdateContractAsSigned(id);
        SendContractSignedNotificationToStaff(id);
        SetAlert("success", "Document signed successfully");
        return Redirect(Request.Headers["Referer"]);

      case "contract_comment":
        if (string.IsNullOrEmpty(content))
          return Redirect(Request.Path);
        AddContractComment(id, content);
        return Redirect(Request.Path + "?tab=discussion");
    }

    // DisableNavigation();
    // DisableSubMenu();

    var data = new
    {
      title = row.contract.Subject,
      contract = self.hooks.apply_filters("contract_html_pdf_data", contract),
      bodyclass = "contract contract-view identity-confirmation",
      identity_confirmation_enabled = true,
      comments = contractsModel.get_comments(id)
    };

    self.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    self.app_css.remove("reset-css", "customers-area-default");
    data = self.hooks.apply_filters("contract_customers_area_view_data", data);

    // NoIndexCustomersArea();
    // View("contracthtml", data);
    // return Layout();
    return Ok();
  }
}
