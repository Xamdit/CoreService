using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;
using Service.Libraries.FormValidations;

namespace Service.Controllers.Core;

public class AppControllerBase(ILogger<MyControllerBase> logger, MyInstance self, MyContext db) : MyControllerBase(logger, self)
{
  public FormValidation form_validation { get; set; }


  public override void Init()
  {
    form_validation = new FormValidation(self.httpContextAccessor);
    self.controller = this;
  }

  public (MyInstance, MyContext ) getInstance()
  {
    return (self, db);
  }
}
