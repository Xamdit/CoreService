using Service.Entities;
using Service.Framework;

namespace Service.Libraries.Sms;

public class SmsTwilio : AppSms
{
  public string sid { get; set; }
  public string token { get; set; }
  public string phone { get; set; }
  public string senderId { get; set; }


  public SmsTwilio(MyInstance instance, MyContext db) : base(instance, db)
  {
    sid = get_option("twilio", "account_sid");
    token = get_option("twilio", "auth_token");
    phone = get_option("twilio", "phone_number");
    senderId = get_option("twilio", "sender_id");
    add_gateway("twilio", new SmsGateway()
    {
      Name = "Twilio",
      Info = "<p>Twilio SMS integration is one way messaging, means that your customers won\"t be able to reply to the SMS. Phone numbers must be in format <a href='https://www.twilio.com/docs/glossary/what-e164' target = '_blank'>E.164</a>.Click <a href = 'https://support.twilio.com/hc/en-us/articles/223183008-Formatting-International-Phone-Numbers' target='_blank'>here</a> to read more how phone numbers should be formatted.</p><hr class='hr-10'/>",
      Options = new List<Trigger>()
      {
        new()
        {
          Label = "account_sid", Value = "Account SID"
        },
        new() { Label = "auth_token", Value = "Auth Token" },
        new() { Label = "phone_number", Value = "Twilio Phone Number" },
        new()
        {
          Label = "sender_id",
          Value = "Alphanumeric Sender ID",
          Info = "<p><a href='https: //www.twilio.com/blog/personalize-sms-alphanumeric-sender-id' target='_blank'>https://www.twilio.com/blog/personalize-sms-alphanumeric-sender-id</a></p>"
        }
      }
    });
  }

  public override bool send(string number, string message)
  {
    return false;
  }
}
