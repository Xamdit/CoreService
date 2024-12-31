using Service.Framework.Core.Engine;

namespace Service.Helpers.Recaptcha;

public static class RecaptchaExtensions
{
  public static bool do_recaptcha_validation(this HelperBase helper, string response)
  {
    return false;
  }
}
