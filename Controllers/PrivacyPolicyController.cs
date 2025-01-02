using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrivacyPolicyController(ILogger<ClientControllerBase> logger, MyInstance self,MyContext db) : ClientControllerBase(logger, self,db)
{
  [HttpGet]
  public IActionResult index()
  {
    data.policy = db.get_option("privacy_policy");
    data.title = label("privacy_policy") + " - " + db.get_option("companyname");
    return MakeSuccess(data);
  }
}
