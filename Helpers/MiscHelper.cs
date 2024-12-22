using Global.Entities;

namespace Service.Helpers;

public static class MiscHelper
{
  /**
 * Generate random alpha numeric string
 * @param  integerlength the length of the string
 * @return string
 */
  public static string generate_two_factor_auth_key()
  {
    // return bin2hex(get_instance()->encryption->create_key(4));
    return "";
  }

  /**
 * Used for estimate and proposal acceptance info array
 * @param  booleanempty should the array values be empty or taken from_POST
 * @return array
 */
  public static Contract get_acceptance_info_array(bool empty = false)
  {
    var _httpContextAccessor = new HttpContextAccessor();
    string signature = null;
    if (_httpContextAccessor.HttpContext.Items.ContainsKey("processed_digital_signature"))
    {
      signature = _httpContextAccessor.HttpContext.Items["processed_digital_signature"] as string;
      _httpContextAccessor.HttpContext.Items.Remove("processed_digital_signature");
    }

    var request = _httpContextAccessor.HttpContext.Request;
    var data = new Contract
    {
      Signature = signature,
      AcceptanceFirstName = empty ? null : request.Form["acceptance_firstname"].ToString(),
      AcceptanceLastName = empty ? null : request.Form["acceptance_lastname"].ToString(),
      AcceptanceEmail = !empty && Convert.ToBoolean(request.Form["acceptance_email"]),
      AcceptanceDate = empty ? null : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
      AcceptanceIp = empty ? null : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
    };

    return data;
  }
}
