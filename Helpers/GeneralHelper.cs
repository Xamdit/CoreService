using System.Dynamic;
using Blazored.LocalStorage;
using Global.Entities;
using Newtonsoft.Json;
using Service.Framework.Core.Engine;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;

namespace Service.Helpers;

public static class GeneralHelper
{
  public static bool isset(object data, string key)
  {
    var jsonString = JsonConvert.SerializeObject(data);
    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
    return json != null && json.ContainsKey(key);
  }

  public static T? value<T>(this HelperBase helper, object data, string key)
  {
    var jsonString = JsonConvert.SerializeObject(data);
    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
    return (T?)Convert.ChangeType(json?[key], typeof(T));
  }


  /**
* Return logged staff User ID from session
* @return mixed
*/
  public static int get_staff_user_id(this HelperBase helper)
  {
    return 0;
  }

  public static int get_staff_user_id(this ILocalStorageService local)
  {
    // var user = local.GetItemAsync<UserSchema>("user").Result;
    // if (user == null) return !is_staff_logged_in() ? 0 : session.GetItemAsync<int>("staff_user_id").Result;
    // var uuid = user.Uuid;
    // var options = new RestClientOptions("https://users.xamdit.com")
    // {
    //   MaxTimeout = -1
    // };
    // var client = new RestClient(options);
    // var request = new RestRequest($"/users/{uuid}", Method.Post);
    // request.AddHeader("accept", "*/*");
    // var response = client.ExecuteAsync(request).Result;
    // var content = response.Content;
    // var dataset = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
    // if (dataset != null && dataset.ContainsKey("user_id")) return Convert.ToInt32(dataset["user_id"]);
    //
    // return !is_staff_logged_in() ? 0 : session.GetItemAsync<int>("staff_user_id").Result;
    return 0;
  }

  /**
 * Is staff logged in
 * @return boolean
 */
  public static bool is_staff_logged_in()
  {
    // return get_instance()->session->has_userdata('staff_logged_in');
    return false;
  }

  /**
* Is client logged in
* @return boolean
*/
  public static bool is_client_logged_in()
  {
    // var client_logged_in = session.GetItemAsync<bool>("client_logged_in").Result;
    // return client_logged_in;
    return false;
  }

  /**
* Get contact user id
* @return mixed
*/
  public static int get_contact_user_id(this HelperBase helper)
  {
    // if (!$CI->session->has_userdata('contact_user_id')) {
    //   return false;
    // }
    // return $CI->session->userdata('contact_user_id');
    return 0;
  }

  /**
 * Generate short_url
 * @since  Version 2.7.3
 *
 * @param  array $data
 * @return mixed Url
 */
  public static string app_generate_short_link(this HelperBase helper, dynamic data)
  {
    // hooks.do_action("before_generate_short_link", data);
    // $accessToken = db.get_option('bitly_access_token');
    //   $client      = new Client();
    //
    // try {
    //   $response = $client->request('POST', 'https://api-ssl.bitly.com/v4/bitlinks', [
    //     'headers' => [
    //   'Authorization' => "Bearer $accessToken",
    //   'Accept'        => 'application/json',
    //     ],
    //   'json' => [
    //   'long_url' => $data['long_url'],
    //   'domain'   => 'bit.ly',
    //   'title'    => $data['title'],
    //     ],
    //     ]);
    //
    //   $response = json_decode($response->getBody());
    //
    //   return $response->link;
    // } catch (RequestException $e) {
    //   log_activity('Bitly ERROR' . (string) $e->getResponse()->getBody());

    return "";
  }

  /**
 * Is user logged in
 * @return boolean
 */
  public static bool is_logged_in(this HelperBase helper)
  {
    return helper.is_client_logged_in() || helper.is_staff_logged_in();
  }

  /**
 * Return logged client User ID from session
 * @return mixed
 */
  public static int get_client_user_id(this HelperBase helper)
  {
    var _is_client_logged_in = helper.is_client_logged_in();
    if (!_is_client_logged_in) return 0;
    // return get_instance()->session->userdata('client_user_id');
    return 10;
  }

  public static bool defined(this HelperBase helper, string key)
  {
    return false;
  }

  public static void define(this HelperBase helper, string key, object value)
  {
  }

  public static void die(this HelperBase helper, string message)
  {
  }

  public static string label(this HelperBase helper, string message)
  {
    return message;
  }

  public static bool is_cron(this HelperBase helper)
  {
    return false;
  }

  /**
 * Archive/remove short url
 * @since  Version 2.7.3
 *
 * @param  string link
 * @return boolean
 */
  public static bool app_archive_short_link(this HelperBase helper, string link)
  {
    var (self, db) = getInstance();
    var accessToken = db.get_option("bitly_access_token");

    if (string.IsNullOrEmpty(accessToken)) return false;

    self.hooks.do_action("before_archive_short_link", link);

    // link = str_replace("https://", "", link);
    link = link.Replace("https://", "");

    var client = new Client();
    try
    {
      // client.patch($"https://api-ssl.bitly.com/v4/bitlinks/{link}", new
      // {
      //   headers = new
      //   {
      //     Authorization = "Bearer accessToken",
      //     Accept = "application/json"
      //   },
      //   json = JsonConvert.SerializeObject(new
      //   {
      //     archived = true
      //   }
      // });

      return true;
    }
    catch (Exception e)
    {
      // log_activity("Bitly ERROR" + e.Message);
    }

    return false;
  }


  /**
 * Get weekdays as array
 * @return array
 */
  public static List<string> get_weekdays(this HelperBase helper)
  {
    return new List<string>
    {
      helper.label("wd_monday"),
      helper.label("wd_tuesday"),
      helper.label("wd_wednesday"),
      helper.label("wd_thursday"),
      helper.label("wd_friday"),
      helper.label("wd_saturday"),
      helper.label("wd_sunday")
    };
  }


  /**
   * Get specific item applied taxes
   * @param  array $item
   * @return mixed
   */
  public static List<Taxis> get_item_taxes(this HelperBase helper, Taxis? item, int item_id)
  {
    var item_taxes = new List<Taxis>();

    if (helper.defined("INVOICE_PREVIEW_SUBSCRIPTION"))
    {
      // item_taxes = item.Name;
    }
    else
    {
      // Separate functions exists to get item taxes for Invoice, Estimate, Proposal, Credit Note
      // var func_taxes = 'get_'. $this->type. '_item_taxes';
      // if (function_exists($func_taxes)) {
      //   item_taxes = call_user_func($func_taxes, $item['id']);
      // }
    }

    return item_taxes;
  }


  /**
 * Is client logged in
 * @return boolean
 */
  public static bool is_client_logged_in(this HelperBase helper)
  {
    var (self, db) = getInstance();
    // return get_instance()->session->has_userdata("client_logged_in");
    return false;
  }

  /**
 * Is staff logged in
 * @return boolean
 */
  public static bool is_staff_logged_in(this HelperBase helper)
  {
    // return get_instance()->session->has_userdata('staff_logged_in');
    return false;
  }

  public static Dictionary<string, object> compact(params (string Name, object Value)[] variables)
  {
    return variables.ToDictionary(v => v.Name, v => v.Value);
  }

  public static void test()
  {
    dynamic _data = new ExpandoObject();
    compact(_data, ("Name", "John"), ("Age", 30));
    Console.WriteLine(JsonConvert.SerializeObject(_data));
  }

  /**
    * Format seconds to H:I:S
    * @param  integer $seconds         mixed
    * @param  boolean $include_seconds
    * @return string
    */
  public static string seconds_to_time_format(double seconds = 0, double include_seconds = 0)
  {
    // return \app\services\utilities\Format::secondsToTime($seconds, $include_seconds);
    return string.Empty;
  }

  public static int total_logged_time(Global.Entities.Task task)
  {
    return 0;
  }

  /**
 * Set current full url to for user to be redirected after login
 * Check below function to see why is this
 */
  public static void redirect_after_login_to_current_url()
  {
    var (self, db) = getInstance();
    var redirectTo = current_full_url();
    // This can happen if at the time you received a notification but your session was expired the system stored this as last accessed URL so after login can redirect you to this URL.
    if (redirectTo.Contains("notifications_check")) return;
    self.session.set_userdata("red_url", redirectTo);

    // get_instance()->session->set_userdata([
    //   'red_url' => $redirectTo,
    //   ]);
  }

  /**
 * Get current url with query vars
 * @return string
 */
  public static string current_full_url()
  {
    var (self, db) = getInstance();
    // var url = self.helper.site_url(self.uri.uri_string();
    // return $_SERVER['QUERY_STRING'] ? $url. '?'. $_SERVER['QUERY_STRING'] : $url;
    return self.helper.base_url();
  }
}
