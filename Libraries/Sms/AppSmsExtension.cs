using Service.Framework.Core.Engine;

namespace Service.Libraries.Sms;

public static class AppSmsExtension
{
  public static AppSms app_sms(this LibraryBase library)
  {
    var (self, db) = getInstance();
    var sms = new SmsTwilio(self, db);
    return sms;
  }
}
