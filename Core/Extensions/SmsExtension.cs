using Service.Framework.Core.Engine;
using Service.Libraries.Sms;

namespace Service.Core.Extensions;

public static class SmsExtension
{
  public static AppSms app_sms(this LibraryBase libs)
  {
    var (self, db) = getInstance();
    var sms = new SmsTwilio(self, db);
    return sms;
  }
}
