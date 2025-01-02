using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.Core;

public class ClientControllerBase(ILogger<MyControllerBase> logger, MyInstance self, MyContext db) : AppControllerBase(logger, self, db)
{
  public override void Init()
  {
  }
}
