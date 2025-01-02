using System.Net;
using Service.Framework.Library.Net;
using Service.Framework.Sessions;

namespace Service.Framework.Core.InputSet;

public class MyInput
{
  private static bool allowGetArray = true;
  private static bool enableXss = false;
  private static bool enableCsrf = false;

  public HttpContext context = null;

  public Dictionary<string, string> headers = new();
  public CookiesManager cookies = new();
  public Session session { get; set; }


  public MyInput()
  {
    // SanitizeGlobals();
    if (enableCsrf && !IsCli()) Security.CsrfVerify();
    // db.log_message("info", "Input Class Initialized");
  }

  public void Init(HttpContext _context)
  {
    context = _context;
  }

  public string cookie(string index = null, bool xssClean = false)
  {
    return FetchFromArray(context.Request.Cookies, index, xssClean);
  }

  public string server(string index, bool xssClean = false)
  {
    return FetchFromHeaders(index, xssClean); // Handle server variables using headers
  }

  private string FetchFromArray(IQueryCollection array, string index = null, bool xssClean = false)
  {
    var value = array[index];
    return xssClean ? Security.XssClean(value) : value;
  }

  private string FetchFromArray(IFormCollection array, string index = null, bool xssClean = false)
  {
    var value = array[index];
    return xssClean ? Security.XssClean(value) : value;
  }

  private string FetchFromArray(IRequestCookieCollection array, string index = null, bool xssClean = false)
  {
    var value = array[index];
    return xssClean ? Security.XssClean(value) : value;
  }

  private string FetchFromHeaders(string index, bool xssClean = false)
  {
    var value = context.Request.Headers[index];
    return xssClean ? Security.XssClean(value) : value;
  }

  public string GetRemoteIPAddress(HttpContext httpContext = null)
  {
    var remoteIpAddress = context?.Connection?.RemoteIpAddress;
    if (remoteIpAddress == null) return "Unknown IP";

    // If the IP is IPv6 loopback (::1), translate it to IPv4 loopback (127.0.0.1).
    if (remoteIpAddress.IsIPv6LinkLocal || remoteIpAddress.IsIPv6Multicast || remoteIpAddress.IsIPv6SiteLocal)
      remoteIpAddress = IPAddress.Loopback;

    return remoteIpAddress.ToString();
  }

  public string ip_address()
  {
    // return self.ip();
    return "0.0.0.0";
  }

  public bool ValidIp(string ip, string type = "")
  {
    return IPAddress.TryParse(ip, out _);
  }

  public string UserAgent(bool xssClean = false)
  {
    return FetchFromHeaders("User-Agent", xssClean);
  }

  public bool IsCli()
  {
    return Environment.UserInteractive;
  }

  public string Method(bool upper = false)
  {
    return upper ? context.Request.Method.ToUpper() : context.Request.Method.ToLower();
  }

  private string StripTags(string input)
  {
    // Implement your strip logic
    return input; // Placeholder: return stripped input
  }

  private class Security
  {
    public static void CsrfVerify()
    {
      // Implement CSRF verification logic
    }

    public static string XssClean(string input)
    {
      // Implement XSS cleaning logic
      return input; // Placeholder: return cleaned input
    }
  }
}
