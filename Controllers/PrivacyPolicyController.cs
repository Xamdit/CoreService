using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrivacyPolicyController(ILogger<MyControllerBase> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index()
  {
    var (self, db) = getInstance();
    data.policy = db.get_option("privacy_policy");
    data.title = self.helper.label("privacy_policy") + " - " + db.get_option("companyname");
    return MakeSuccess(data);
  }
}
