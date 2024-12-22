using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.Core;

public class AppControllerBase(ILogger<MyControllerBase> logger, MyInstance self) : MyControllerBase(logger, self)
{
  public Libraries.FormValidation form_validation { get; set; }

  public override void Init()
  {
    form_validation = new Libraries.FormValidation(self.httpContextAccessor);
    self.controller = this;
  }
}
