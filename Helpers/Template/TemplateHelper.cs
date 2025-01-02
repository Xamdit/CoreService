using System.Text.RegularExpressions;
using Service.Framework.Core.Engine;

namespace Service.Helpers.Template;

public static class TemplateHelper
{
  public static string clear_textarea_breaks(  string text, string replace = "")
  {
    if (string.IsNullOrEmpty(text)) return text;
    var breaks = new[] { "<br />", "<br>", "<br/>" };
    text = breaks.Aggregate(text, (current, br) => current.Replace(br, replace, StringComparison.OrdinalIgnoreCase));
    return text.Trim();
  }

  public static string Nl2BrSaveHtml(  string content)
  {
    if (!Regex.IsMatch(content, @"<\/.*>")) return content.Replace("\n", "<br />");
    var lines = content.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.None);
    return lines.Aggregate(
      string.Empty, (current, line) => current + (line.EndsWith(">")
          ? line
          : line + "<br />"
        )
    );
  }

  public static void AppJsAlerts(this HelperBase helper)
  {
    var (self, db) = getInstance();
    // var alertClass = GetAlertClass();
    var alertClass = string.Empty;
    var script = string.Empty;
    if (!string.IsNullOrEmpty(self.input.session.get_string("system-popup")))
    {
      var message = self.input.session.get_string("system-popup");
      script = $@"<script>
                                $(function() {{
                                    var popupData = {{
                                        message: {message}
                                    }};
                                    system_popup(popupData);
                                }});
                            </script>";
      // context.Response.WriteAsync(script);
    }

    if (string.IsNullOrEmpty(alertClass)) return;

    var alertMessage = self.input.session.get_string($"message-{alertClass}");
    script = $@"<script>
                                $(function() {{
                                    alert_float('{alertClass}', '{alertMessage}');
                                }});
                            </script>";
    // self.httpContextAccessor.Response.WriteAsync(script);
  }

  public static string GetCompanyLogo(this HelperBase helper, string uri = "", string hrefClass = "", string type = "")
  {
    var (self, db) = getInstance();
    var companyLogo = db.config("company_logo");
    var companyName = db.config("companyname");
    var logoUrl = string.IsNullOrEmpty(uri) ? "/" : uri;

    if (!string.IsNullOrEmpty(companyLogo)) return $"<a href='{logoUrl}' class='logo img-responsive {hrefClass}'><img src='/uploads/company/{companyLogo}' alt='{companyName}' class='img-responsive'></a>";

    return !string.IsNullOrEmpty(companyName)
      ? $"<a href='{logoUrl}' class='{hrefClass} logo logo-text'>{companyName}</a>"
      : string.Empty;
  }

  public static string StripTags(this HelperBase helper, string html)
  {
    var allowedTags = new[] { "<br>", "<em>", "<p>", "<ul>", "<ol>", "<li>", "<h4>", "<h3>", "<h2>", "<h1>", "<pre>", "<code>", "<a>", "<img>", "<strong>", "<b>", "<blockquote>", "<table>", "<thead>", "<th>", "<tr>", "<td>", "<tbody>", "<tfoot>" };
    return string.Join(" ", allowedTags); // Implementation may vary.
  }
}
