using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.admin;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController(ILogger<MyControllerBase> logger, MyInstance self) : ClientControllerBase(logger, self)
{
}