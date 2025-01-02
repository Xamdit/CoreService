using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Service.Framework.Core.Engine;

namespace Service.Framework.Core.Extensions;

public static class StringExtension
{
  public static MarkupString markup(this HelperBase self, string str)
  {
    return new MarkupString(str);
  }

  public static string base64(this HelperBase self, string originalString, bool encode = true)
  {
    if (encode)
    {
      var bytesToEncode = Encoding.UTF8.GetBytes(originalString);
      return Convert.ToBase64String(bytesToEncode);
    }

    var bytes = Convert.FromBase64String(originalString);
    return Encoding.UTF8.GetString(bytes);
  }

  public static bool strpos(this HelperBase self, string str, string substr)
  {
    return !string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(substr) && str.IndexOf(substr, StringComparison.Ordinal) > 0;
  }

  public static string uuid( )
  {
    return Guid.NewGuid().ToString("n");
  }

  public static bool is_valid_email(this HelperBase self, string email)
  {
    var pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    var isValid = Regex.IsMatch(email, pattern);
    return isValid;
  }

  public static bool start_with(this HelperBase self, string haystack, string needle)
  {
    return haystack.StartsWith(needle);
  }

  public static bool ends_with(this HelperBase self, string haystack, string needle)
  {
    return haystack.EndsWith(needle);
  }

  public static string strafter(  string str, string substring)
  {
    var index = str.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
    return index >= 0 ? str[(index + substring.Length)..] : str;
  }

  public static string strbefore(this HelperBase helperBase, string str, string substring)
  {
    var index = str.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
    return index >= 0 ? str[..index] : str;
  }

  public static bool is_connected(this HelperBase self, string domain = "www.google.com")
  {
    using var ping = new Ping();
    try
    {
      var reply = ping.Send(domain);
      return reply?.Status == IPStatus.Success;
    }
    catch (PingException)
    {
      return false;
    }
  }

  public static string str_lreplace(this HelperBase self, string subject, string search, string replace)
  {
    var pos = subject.LastIndexOf(search, StringComparison.OrdinalIgnoreCase);
    return pos < 0 ? subject : subject[..pos] + replace + subject[(pos + search.Length)..];
  }

  public static string get_string_between(  string str, string start, string end)
  {
    var startIndex = str.IndexOf(start, StringComparison.OrdinalIgnoreCase);
    if (startIndex < 0) return string.Empty;
    startIndex += start.Length;
    var endIndex = str.IndexOf(end, startIndex, StringComparison.OrdinalIgnoreCase);
    return endIndex >= 0 ? str.Substring(startIndex, endIndex - startIndex) : string.Empty;
  }

  public static string time_ago_specific(this HelperBase self, DateTime lastReply, string from = "now")
  {
    var timeSpan = DateTime.Now - lastReply;
    return timeSpan.TotalMilliseconds < 0 ? "0" : timeSpan.Hours.ToString();
  }

  public static int sec2_qty(dynamic sec)
  {
    return 0;
  }

  public static string seconds_to_time_format(this HelperBase self, double seconds, bool includeSeconds = false)
  {
    var timeSpan = TimeSpan.FromSeconds(seconds);
    var format = includeSeconds ? @"hh\:mm\:ss" : @"hh\:mm";
    return timeSpan.ToString(format);
  }

  public static int hours_to_seconds_format(this HelperBase self, string hours)
  {
    var timeSpan = TimeSpan.Parse(hours, CultureInfo.InvariantCulture);
    return (int)timeSpan.TotalSeconds;
  }

  public static bool ip_in_range(this HelperBase self, string ip, string range)
  {
    return false;
  }

  public static Dictionary<string, T> array_merge_recursive_distinct<T>(
    this HelperBase self,
    Dictionary<string, T> array1,
    Dictionary<string, T> array2)
  {
    var result = new Dictionary<string, T>(array1);
    foreach (var item in array2.Where(item => !result.ContainsKey(item.Key))) result.Add(item.Key, item.Value);
    return result;
  }

  public static Dictionary<string, object> array_to_object(this HelperBase self, params object[] array)
  {
    var output = new Dictionary<string, object>();
    array.ToList()
      .ForEach(x =>
      {
        var jsonString = JsonConvert.SerializeObject(x);
        var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        foreach (var kvp in item)
        {
          var key = kvp.Key;
          var value = kvp.Value;
          if (output.ContainsKey(key)) output.Remove(key);
          output[key] = value;
        }
      });
    return output;
  }

  public static T? array_to_object<T>(this HelperBase helper, T[] array) where T : class
  {
    var output = helper.array_to_object(array);
    var jsonString = JsonConvert.SerializeObject(output);
    var dataset = JsonConvert.DeserializeObject<T>(jsonString);
    return dataset;
  }

  public static IEnumerable<T> array_flatten<T>(this HelperBase self, IEnumerable<IEnumerable<T>> array)
  {
    return array.SelectMany(x => x);
  }

  public static bool value_exists_in_array_by_key<T>(this HelperBase self, IEnumerable<Dictionary<string, T>> array, string key, T val)
  {
    return array.Any(item => item.ContainsKey(key) && item[key].Equals(val));
  }

  public static bool in_array_multidimensional<T>(this HelperBase self, IEnumerable<Dictionary<string, T>> array, string key, T val)
  {
    return array.Any(item => item.ContainsKey(key) && item[key].Equals(val));
  }

  public static bool in_object_multidimensional<T>(this HelperBase self, IEnumerable<T> obj, string key, T val)
  {
    return (from item in obj
      let property = item.GetType().GetProperty(key)
      where property != null && property.GetValue(item).Equals(val)
      select item).Any();
  }

  public static List<T> array_pluck<T>(this HelperBase self, IEnumerable<Dictionary<string, T>> array, string key)
  {
    return array.Select(item => item.ContainsKey(key) ? item[key] : default).ToList();
  }

  public static string adjust_color_brightness(this HelperBase self, string hex, int steps)
  {
    return string.Empty;
  }

  public static string hex2rgb(this HelperBase self, string color)
  {
    return string.Empty;
  }

  /**
       * Check for links/emails/ftp in string to wrap in href
       * @param  string $ret
       * @return string      formatted string with href in any found
       */
  public static string check_for_links(this HelperBase helper, string input)
  {
    if (string.IsNullOrEmpty(input)) return input;

    // Pattern to match URLs, emails, and FTP links
    var pattern = @"((http|https|ftp)://[^\s]+)|(\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b)";

    // Replace matched links/emails with wrapped href
    var formatted = Regex.Replace(input, pattern, match =>
    {
      var url = match.Value;
      // Check if it's an email
      return url.Contains("@")
        ? $"<a href=\"mailto:{url}\">{url}</a>"
        : $"<a href=\"{url}\" target=\"_blank\">{url}</a>";
    });

    return formatted;
  }

  // public static string time_ago(this HelperBase self, DateTime date)
  // {
  //   var timeSpan = DateTime.Now - date;
  //   return timeSpan.TotalMilliseconds < 0 ? "0" : timeSpan.Hours.ToString();
  // }

  public static string db.slug_it(this HelperBase self, string str, string lang = "")
  {
    return string.Empty;
  }

  public static float similarity(this HelperBase self, string str1, string str2)
  {
    return 0;
  }

  public static string strip_tags(this HelperBase self, string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    var strippedString = Regex.Replace(input, "<.*?>", string.Empty);
    strippedString = WebUtility.HtmlDecode(strippedString);
    return strippedString;
  }

  // public static string nl2br(string str)
  // {
  //   return str.Replace("\r\n", "<br>").Replace("\n", "<br>");
  // }

  public static string nl2br(this string str)
  {
    return str.Replace("\r\n", "<br>").Replace("\n", "<br>");
  }

  public static string nl2br_save_html(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    var brText = input.Replace("\n", "<br>");
    var encodedText = WebUtility.HtmlEncode(brText)
      .Replace("&lt;br&gt;", "<br>")
      .Replace("&lt;br /&gt;", "<br />");
    return encodedText;
  }

  public static bool is_html(this HelperBase self, string str)
  {
    return !string.IsNullOrEmpty(str) && str.IndexOf("<html>", StringComparison.OrdinalIgnoreCase) >= 0;
  }

  public static string html_escape(this HelperBase self, string input)
  {
    return string.IsNullOrEmpty(input) ? input : HttpUtility.HtmlEncode(input);
  }

  public static string generate_two_factor_auth_key(this HelperBase self)
  {
    var random = new Random();
    var characters = "0123456789abcdefghijklmnopqrstuvwxyz";
    const int length = 16;
    var key = new char[length];
    for (var i = 0; i < length; i++) key[i] = characters[random.Next(0, characters.Length)];
    key[length - 1] = Guid.NewGuid().ToString("N")[0];
    return new string(key);
  }

  public static string strtoupper(this HelperBase self, string str)
  {
    return str.ToUpper();
  }

  public static string html_encode(this HelperBase self, string input)
  {
    return WebUtility.HtmlEncode(input);
  }

  public static string strtolower(this HelperBase self, string str)
  {
    return str.ToLower();
  }

  public static bool empty(this HelperBase self, object? input)
  {
    return string.IsNullOrEmpty(Convert.ToString(input));
  }

  public static string str_ireplace(this HelperBase self, string search, string replace, string subject)
  {
    return Regex.Replace(subject, Regex.Escape(search), replace, RegexOptions.IgnoreCase);
  }

  public static bool is_base64_string(this HelperBase self, string base64String)
  {
    if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0) return false;
    return Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,2}$");
  }

  public static string get_string_between_inclusive(this HelperBase self, string str, string start, string end)
  {
    var startIndex = str.IndexOf(start, StringComparison.OrdinalIgnoreCase);
    if (startIndex < 0) return string.Empty;
    var endIndex = str.IndexOf(end, startIndex + start.Length, StringComparison.OrdinalIgnoreCase);
    return endIndex >= 0 ? str.Substring(startIndex, endIndex - startIndex + end.Length) : string.Empty;
  }

  // Method to format double
  public static string number_format( double number, int digit = 2)
  {
    return number_format((decimal)number, digit);
  }

  // Method to format int
  public static string number_format( decimal percentage, int number, int digit = 2)
  {
    return number_format((decimal)number, digit);
  }

  // Method to format float
  public static string number_format( float number, int digit = 2)
  {
    return number_format((decimal)number, digit);
  }

  public static string number_format( decimal number, int digit = 2)
  {
    // Create a custom NumberFormatInfo object
    var customFormat = new NumberFormatInfo
    {
      NumberDecimalDigits = digit, // Number of decimal places
      NumberGroupSeparator = ",", // Group separator
      NumberDecimalSeparator = "." // Decimal separator
    };
    // Format the number using the custom format
    return number.ToString("N", customFormat);
  }

  public static string md5(this HelperBase helperBase, string input)
  {
    // Check if the input is null or empty
    if (string.IsNullOrEmpty(input))
      throw new ArgumentException("Input cannot be null or empty", nameof(input));

    // Create an MD5 hash algorithm instance
    using var md5 = MD5.Create();
    // Convert the input string to a byte array
    var inputBytes = Encoding.UTF8.GetBytes(input);

    // Compute the hash
    var hashBytes = md5.ComputeHash(inputBytes);

    // Convert the byte array to a hexadecimal string
    var sb = new StringBuilder();
    foreach (var b in hashBytes) sb.Append(b.ToString("x2"));

    return sb.ToString();
  }

  public static string preg_replace(string pattern, string replacement, string subject)
  {
    return Regex.Replace(subject, pattern, replacement);
  }

  public static bool IsNullOrEmpty(this string value)
  {
    return string.IsNullOrEmpty(value);
  }

  public static bool Real(this string value)
  {
    return Convert.ToBoolean(value);
  }

  public static bool Is(this string value, bool checker)
  {
    return Convert.ToBoolean(value) == checker;
  }

  public static List<List<string>> preg_match_all(string pattern, string input)
  {
    var matchesList = new List<List<string>>();
    var regex = new Regex(pattern);
    var matches = regex.Matches(input);

    foreach (Match match in matches)
    {
      var groupList = new List<string>();
      foreach (Group group in match.Groups)
        groupList.Add(group.Value);
      matchesList.Add(groupList);
    }

    return matchesList;
  }
}
