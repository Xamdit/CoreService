using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;
using Service.Libraries.FormValidations;

namespace Service.Controllers.Core;

public class AppControllerBase(ILogger<MyControllerBase> logger, MyInstance self) : MyControllerBase(logger, self)
{
  public FormValidation form_validation { get; set; }

  public MyContext db
  {
    get
    {
      var (s, _db) = getInstance();
      return _db;
    }
  }

  public override void Init()
  {
    form_validation = new FormValidation(self.httpContextAccessor);
    self.controller = this;
  }
}
