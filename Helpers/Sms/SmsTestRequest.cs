namespace Service.Helpers.Sms;

public class SmsTestRequest
{
  public string Id { get; set; }
  public string Number { get; set; } = "0000000000";
  public string Message { get; set; } = "";
  public bool SmsGatewayTest { get; set; }
}
