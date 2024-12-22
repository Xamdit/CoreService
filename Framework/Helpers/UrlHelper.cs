using System.Text.RegularExpressions;
using Service.Framework.Core.Engine;

namespace Service.Framework.Helpers;

public static class UrlHelper
{
  public static string admin_url(this HelperBase helper, string uri = "", string protocol = null)
  {
    var output = new List<string> { "admin" };
    uri
      .Split("/").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList()
      .ForEach(output.Add);
    return helper.site_url(string.Join("/", output), protocol);
  }

  public static string client_url(this HelperBase helper, string uri = "", string protocol = null)
  {
    var output = new List<string> { "client" };
    uri
      .Split("/").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList()
      .ForEach(output.Add);
    return helper.site_url(string.Join("/", output), protocol);
  }

  public static string site_url(this HelperBase helper, string uri = "", string protocol = null)
  {
    return helper.get_base_url() + uri;
  }

  public static string base_url(this HelperBase helper, string uri = "", string protocol = null)
  {
    return helper.get_base_url() + uri;
  }

  /**
 * Return admin URI
 * CUSTOM_ADMIN_URL is not yet tested well, don't define it
 * @return string
 */
  public static string get_admin_uri()
  {
    // return ADMIN_URI;
    return "https://localhost:5001/admin";
  }

  public static string get_client_uri()
  {
    // return ADMIN_URI;
    return "https://localhost:5001/client";
  }

  public static string site_url(this HelperBase helper, string url)
  {
    return url;
  }


  public static string current_url(this HelperBase helper)
  {
    var self = MyInstance.Instance;
    var context = self.context;
    return context.Request.Path;
  }

  public static string uri_string(this HelperBase helper)
  {
    var self = MyInstance.Instance;
    var context = self.context;
    return context.Request.Path.Value;
  }

  public static string index_page(this HelperBase helper)
  {
    return "index";
  }

  public static string anchor(this HelperBase helper, string uri = "", string title = "", string attributes = "")
  {
    var siteUrl = helper.is_absolute_url(uri) ? uri : helper.site_url(uri);
    if (string.IsNullOrEmpty(title)) title = siteUrl;

    return $"<a href=\"{siteUrl}\" {attributes}>{title}</a>";
  }

  public static string anchor_popup(this HelperBase helper, string uri = "", string title = "", object attributes = null)
  {
    var siteUrl = helper.is_absolute_url(uri) ? uri : helper.site_url(uri);
    if (string.IsNullOrEmpty(title)) title = siteUrl;

    var windowName = "_blank";
    var additionalAttributes = string.Empty;

    if (attributes != null) additionalAttributes = helper.stringify_attributes(attributes);

    return $"<a href=\"{siteUrl}\" onclick=\"window.open('{siteUrl}', '{windowName}'); return false;\" {additionalAttributes}>{title}</a>";
  }

  public static string mail_to(this HelperBase helper, string email, string title = "", string attributes = "")
  {
    if (string.IsNullOrEmpty(title)) title = email;

    return $"<a href=\"mailto:{email}\" {attributes}>{title}</a>";
  }

  public static string safe_mail_to(this HelperBase helper, string email, string title = "", string attributes = "")
  {
    return "";
  }

  public static string auto_link(this HelperBase helper, string str, string type = "both", bool popup = false)
  {
    if (type != "email")
      str = Regex.Replace(str, @"(\w*://|www\.)[a-z0-9]+(-+[a-z0-9]+)*(\.[a-z0-9]+(-+[a-z0-9]+)*)+(/([^\s()<>;]+\w)?/?)?", match =>
      {
        var target = popup ? " target=\"_blank\" rel=\"noopener\"" : "";
        return $"<a href=\"{(match.Value.StartsWith("/") ? "" : "http://")}{match.Value}\"{target}>{match.Value}</a>";
      });

    if (type != "url")
    {
      // Handle email linking
    }

    return str;
  }

  public static string prep_url(this HelperBase helper, string str = "")
  {
    if (str == "http://" || str == "") return string.Empty;

    if (!Uri.TryCreate(str, UriKind.Absolute, out var uriResult)) return "http://" + str;

    return str;
  }

  public static string url_title(this HelperBase helper, string str, string separator = "-", bool lowercase = false)
  {
    var qSeparator = Regex.Escape(separator);
    var trans = new Dictionary<string, string>
    {
      { "&.+?;", "" },
      { "[^\\w\\d _-]", "" },
      { "\\s+", separator },
      { $"({qSeparator})+", separator }
    };

    str = trans.Keys.Aggregate(str, (current, key) => Regex.Replace(current, key, trans[key]));

    if (lowercase) str = str.ToLowerInvariant();

    return str.Trim(separator.ToCharArray());
  }

  public static void redirect(this HelperBase helper, string uri = "", string method = "auto", int? code = null)
  {
    // Redirect logic here
  }

  private static string get_base_url(this HelperBase helper)
  {
    var self = MyInstance.Instance;
    var request = self.context.Request;
    var currentUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

    return currentUrl;
  }

  private static bool is_absolute_url(this HelperBase helper, string url)
  {
    return Regex.IsMatch(url, @"^(\w+:)?//");
  }

  private static string stringify_attributes(this HelperBase helper, object attributes)
  {
    return "";
  }
}
