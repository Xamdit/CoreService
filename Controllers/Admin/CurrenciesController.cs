using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Models;

namespace Service.Controllers.Admin;

[ApiController]
[Route("api/admin/currencies")]
public class CurrenciesController(ILogger<CurrenciesController> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{
  private CurrenciesModel currencies_model;

  public override void Init()
  {
    currencies_model = self.currencies_model(db);

    if (!db.is_admin())
      access_denied("Currencies");
  }

  /* List all currencies */
  public IActionResult index()
  {
    // if (self.input.is_ajax_request())
    //   this.app.get_table_data("currencies");
    data.title = label("currencies");
    return MakeResult(data);
  }

  /* Update currency or add new / ajax */
  [HttpGet("manage")]
  public IActionResult manage_get()
  {
    return Ok();
  }

  [HttpPost("manage")]
  public IActionResult manage()
  {
    var currency = self.input.post<Currency>();
    if (currency == null)
    {
      var success = currencies_model.add(data);
      var message = "";
      if (success == true) message = label("added_successfully", label("currency"));
      return MakeResult(new
      {
        success,
        message
      });
    }
    else
    {
      var success = currencies_model.edit(data);
      var message = "";
      if (success == true) message = label("updated_successfully", label("currency"));
      return MakeResult(new { success, message });
    }
  }

  /* Make currency your base currency */
  public IActionResult make_base_currency(int? id)
  {
    if (!id.HasValue)
      return Redirect(admin_url("currencies"));
    var response = currencies_model.make_base_currency(id.Value);
    if (response.is_success && response.has_transactions_currency) set_alert("danger", label("has_transactions_currency_base_change"));
    else if (response.is_success == true) set_alert("success", label("base_currency_set"));
    return Redirect(admin_url("currencies"));
  }

  /* Delete currency from database */
  public IActionResult delete(int? id)
  {
    if (!id.HasValue)
      return Redirect(admin_url("currencies"));
    var response = currencies_model.delete(id.Value);
    if (response.is_success && response.referenced)
      set_alert("warning", label("is_referenced", label("currency_lowercase")));
    else if (response.is_success && response.is_default)
      set_alert("warning", label("cant_delete_base_currency"));
    else if (response.is_success)
      set_alert("success", label("deleted", label("currency")));
    else
      set_alert("warning", label("problem_deleting", label("currency_lowercase")));
    return Redirect(admin_url("currencies"));
  }

  /* Get symbol by currency id passed */
  public IActionResult get_currency_symbol(int id)
  {
    if (self.input.is_ajax_request())
      return MakeResult(new
      {
        symbol = currencies_model.get_currency_symbol(id)
      });
    return Ok();
  }
}
