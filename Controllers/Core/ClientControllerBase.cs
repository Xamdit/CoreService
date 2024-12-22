using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.Core;

public class ClientControllerBase(ILogger<MyControllerBase> logger, MyInstance self) : AppControllerBase(logger, self)
{
  public override void Init()
  {
  }
}
