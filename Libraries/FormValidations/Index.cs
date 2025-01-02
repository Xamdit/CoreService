using Service.Framework.Core.Engine;

namespace Service.Libraries.FormValidations;

public static class Index
{
  private static FormValidation instance { get; set; } = null;

  public static FormValidation form_validation(this LibraryBase library)
  {
    if (instance == null)
      instance = new FormValidation(self.httpContextAccessor);
    return instance;
  }
}
