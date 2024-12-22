using Global.Entities;
using Service.Framework;

namespace Service.Libraries.Sms;

internal class SmsClickatell : AppSms
{
  private string api_key = string.Empty;

  private string requestURL = "https://platform.clickatell.com/messages/http/send";

  public SmsClickatell(MyInstance instance, MyContext db) : base(instance, db)
  {
    api_key = get_option("clickatell", "api_key");
    add_gateway("clickatell", new SmsGateway
    {
      Info = "<p>Clickatell SMS integration is one way messaging, means that your customers won't be able to reply to the SMS.</p><hr class='hr-10'>",
      Name = "Clickatell",
      Options = new List<Trigger>()
      {
        new()
        {
          Label = "api_key",
          Value = "API Key"
        }
      }
    });
  }

  public override bool send(string number, string message)
  {
    return false;
  }
}
