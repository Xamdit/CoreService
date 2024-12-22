using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Framework;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CronController(ILogger<ContractController> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index(string key)
  {
    var (self, db) = getInstance();
    db.update_option("cron_has_run_from_cli", 1);

    if (self.helper.defined("APP_CRON_KEY") && self.globals("APP_CRON_KEY") != key)
      // header('HTTP/1.0 401 Unauthorized');
      return MakeError("Passed cron job key is not correct. The cron job key should be the same like the one defined in APP_CRON_KEY constant.");
    var last_cron_run = db.get_option("last_cron_run");
    var seconds = self.hooks.apply_filters("cron_functions_execute_seconds", 300);
    var last_cron_date = DateTime.Parse(last_cron_run + seconds);
    if (!string.IsNullOrEmpty(last_cron_run) && DateTime.Now <= last_cron_date) return Ok();
    var cron_model = self.model.cron_model();
    cron_model.run();
    return Ok();
  }
}
