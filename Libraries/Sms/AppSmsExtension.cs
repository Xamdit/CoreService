using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Libraries.Sms;

public static class AppSmsExtension
{
  public static AppSms app_sms(this LibraryBase library)
  {
    var self = new MyInstance();
    var db = new MyContext();
    var sms = new SmsTwilio(self, db);
    return sms;
  }
}
