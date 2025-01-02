using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.Core;

public class AdminControllerBase(ILogger<MyControllerBase> logger, MyInstance self, MyContext db) : MyControllerBase(logger, self)
{
  public override void Init()
  {
  }
}
