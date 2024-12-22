using Service.Framework.Core.Engine;

namespace Service.Libraries.FormValidations;

public static class Index
{
  public static FormValidation form_validation(this LibraryBase library)
  {
    var (self, db) = getInstance();
    return new FormValidation(self.httpContextAccessor);
  }
}
