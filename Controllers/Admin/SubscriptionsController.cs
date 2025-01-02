using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController(ILogger<MyControllerBase> logger, MyInstance self,MyContext db) : ClientControllerBase(logger, self,db)
{
}
