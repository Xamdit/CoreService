using Newtonsoft.Json;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;
using Service.Framework.Core.InputSet;
using static Service.Helpers.Template.TemplateHelper;

namespace Service.Helpers.Sms;

public static class SmsHelper
{
  // hooks().add_action("admin_init", "maybe_test_sms_gateway");
  public static string maybe_test_sms_gateway(this MyContext db, SmsTestRequest request)
  {
    if (!db.is_staff_logged_in() || !request.SmsGatewayTest) return string.Empty;
    var gateway = db.get_sms_gateway(request.Id);
    if (gateway == null) return JsonConvert.SerializeObject(new { success = false, error = "SMS gateway not found." });
    gateway.SetTestMode(true);
    // Send the SMS
    var result = gateway.Send(request.Number, clear_textarea_breaks(request.Message));
    // Prepare the response
    var response = new { success = false };

    // if (SmsError != null) // Assuming SmsError is globally available or passed along
    //   return JsonConvert.SerializeObject(new { success = false, error = SmsError });

    gateway.SetTestMode(false);
    // return JsonConvert.SerializeObject(new { success = true });
    return "Unauthorized or invalid request";
  }

  private static ISmsGateway get_sms_gateway(this MyContext db, string gatewayId)
  {
    // Retrieve the correct SMS gateway based on the ID
    // Example: return _smsGatewayFactory.GetGatewayById(gatewayId);
    return null; // Replace with actual gateway retrieval logic
  }

  public static void maybe_test_sms_gateway(this MyContext db)
  {
    if (!db.is_staff_logged_in() || string.IsNullOrEmpty(self.input.post("sms_gateway_test")))
      return;
    // gateway = self.{"sms_" . self.input.post("id")};
    // gateway.set_test_mode(true);
    // var retval = gateway.send(
    //   self.input.post("number"),
    //   clear_textarea_breaks( (self.input.post("message").nl2br()))
    // );

    dynamic response = new { success = false };
    // if (isset(GLOBALS["sms_error"]))
    //   response.error = GLOBALS["sms_error"];
    // else
    //   response.success = true;
    // gateway.set_test_mode(false);
    //
    // echo json_encode(response);
    // die;
  }

  // hooks().add_action("admin_init", "_maybe_sms_gateways_settings_group");

  // hooks().add_action("app_init", "app_init_sms_gateways");

  private static void app_init_sms_gateways(this MyInstance self)
  {
    var gateways = new List<string>
    {
      "sms/sms_clickatell",
      "sms/sms_msg91",
      "sms/sms_twilio"
    };

    gateways = hooks.apply_filters("sms_gateways", gateways);

    // foreach (var gateway in gateways  )
    //   self.load.library(gateway);
  }

  public static bool is_sms_trigger_active(this HelperBase helper, string trigger = "")
  {
    // var active = app_sms.get_active_gateway();
    // return !active ? false : app_sms.is_trigger_active(trigger);
    return false;
  }


  public static bool can_send_sms_based_on_creation_date(this HelperBase helper, DateTime data_date_created)
  {
    var now = DateTime.Now;
    var datediff = now - data_date_created;
    var days_diff = Math.Floor(datediff.TotalDays); // TotalDays gives the difference in days as a double
    return days_diff < DO_NOT_SEND_SMS_ON_DATA_OLDER_THEN || days_diff == DO_NOT_SEND_SMS_ON_DATA_OLDER_THEN;
  }
}
