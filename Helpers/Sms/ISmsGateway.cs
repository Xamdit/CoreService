namespace Service.Helpers.Sms;

public interface ISmsGateway
{
  // Set test mode for the gateway
  void SetTestMode(bool isTestMode);

  // Send SMS message
  bool Send(string phoneNumber, string message);

  // Check if the SMS trigger is active
  bool IsTriggerActive(string trigger);

  // Get the gateway name
  string GetGatewayName();
}
