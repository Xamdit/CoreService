using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TermsAndConditionsController(ILogger<MyControllerBase> logger, MyInstance self) : ClientControllerBase(logger, self)
{
  [HttpGet]
  public IActionResult index()
  {
    var (self, db) = getInstance();
    // data.terms = self.helper.get_option("terms_and_conditions");
    // data.title = _l("terms_and_conditions") + " - " + self.helper.get_option("companyname");
    // this.data(data);
    // this.view('terms_and_conditions');
    // this.layout();
    return Ok();
  }
}
