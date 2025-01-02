using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TermsAndConditionsController(ILogger<ClientControllerBase> logger, MyInstance self,MyContext db) : ClientControllerBase(logger, self,db)
{
  [HttpGet]
  public IActionResult index()
  {
    // data.terms = self.helper.get_option("terms_and_conditions");
    // data.title = _l("terms_and_conditions") + " - " + self.helper.get_option("companyname");
    // this.data(data);
    // this.view('terms_and_conditions');
    // this.layout();
    return Ok();
  }
}
