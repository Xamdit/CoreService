using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;
using static Service.Helpers.FileHelper;

namespace Service.Controllers.Admin.AutoUpdate;

[ApiController]
[Route("api/admin/auto-update")]
public class AutoUpdateController(ILogger<AutoUpdateController> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{
  public override void Init()
  {
  }

  public IActionResult index([FromBody] AutoUpdateSchema schema)
  {
    var purchase_key = schema.purchase_key.Trim();
    var latest_version = schema.latest_version;
    var UPDATE_URL = "";
    var url = $"{UPDATE_URL}?purchase_key={purchase_key}";

    hooks.do_action("before_perform_update", latest_version);
    var tmp_dir = get_temp_dir();

    if (!string.IsNullOrEmpty(tmp_dir) || !is_writable(tmp_dir))
      tmp_dir = app_temp_dir();

    // try
    // {
    //   var config = new app\services\upgrade\Config(
    //     purchase_key,
    //     latest_version,
    //     this.current_db_version,
    //     url,
    //     tmp_dir,
    //     FCPATH
    //   );
    //
    //   if (this.input.post('upgrade_function') == = 'old')
    //   {
    //     adapter = new app\services\upgrade\CurlCoreUpgradeAdapter();
    //   }
    //   else
    //   {
    //     adapter = new app\services\upgrade\GuzzleCoreUpgradeAdapter();
    //   }
    //
    //   adapter.setConfig(config);
    //   upgrade = new app\services\upgrade\UpgradeCore(adapter);
    //   upgrade.perform();
    // }
    // catch (Exception e)
    // {
    //   // header('HTTP/1.0 400 Bad error');
    //   return MakeError([e.Message]);
    // }

    return Ok();
  }

  // Temporary function for v1.7.0 will be removed in a future, or perhaps not?
  public IActionResult database()
  {
    return Ok();
  }
}
