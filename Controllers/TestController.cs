using Microsoft.AspNetCore.Mvc;

namespace Service.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
  [HttpGet]
  public IActionResult Index()
  {
    return Ok("Hello World");
  }
}
