using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.admin;

[ApiController]
[Route("api/admin/clients")]
public class ClientsController(ILogger<MyControllerBase> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{





}
