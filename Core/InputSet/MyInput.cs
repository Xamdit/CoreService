using System.Net;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Framework.Core.Extensions;
using Service.Framework.Library.Net;
using Service.Framework.Sessions;

namespace Service.Core.InputSet;

public class MyInput
{
  private static string ipAddress = string.Empty;
  private static bool allowGetArray = true;
  private static bool enableXss = false;
  private static bool enableCsrf = false;

  private static string rawInputStream = string.Empty;
  private static Dictionary<string, string> inputStream = new();
  private HttpContext context = null;
  public Dictionary<string, string> _get = new();
  public Dictionary<string, string> _post = new();
  public Dictionary<string, string> headers = new();
  public CookiesManager cookies = new();
  public Session session { get; set; }


  public MyInput()
  {
    var (self, db) = getInstance();
    // SanitizeGlobals();
    if (enableCsrf && !IsCli()) Security.CsrfVerify();
    self.helper.log_message("info", "Input Class Initialized");
  }

  public void Init(HttpContext _context)
  {
    var (self, db) = getInstance();
    if (context != null) return;
    context = _context;
    self.ignore(() => { context.Request.Form.Keys.ToList().ForEach(key => { post(key, context.Request.Form[key]); }); });
    self.ignore(() => { context.Request.Query.Keys.ToList().ForEach(key => { get(key, context.Request.Query[key]); }); });
    // self.ignore(() => { context.Request.Cookies.Keys.ToList().ForEach(key => { cookies[key] = context.Request.Cookies[key]; }); });
    self.ignore(() => { context.Request.Headers.Keys.ToList().ForEach(key => { headers[key] = context.Request.Headers[key]; }); });
  }

  public Dictionary<string, string> post()
  {
    return _post;
  }

  public string post(string key)
  {
    return _post[key];
  }

  public T post<T>(string key)
  {
    var output = _post[key];
    return (T)Convert.ChangeType(output, typeof(T));
  }

  public T? post<T>()
  {
    var jsonString = JsonConvert.SerializeObject(_post);
    var dataset = JsonConvert.DeserializeObject<T>(jsonString);
    return dataset;
  }

  public void post(string key, string value)
  {
    _post[key] = value;
  }

  public Dictionary<string, string> get()
  {
    return _get;
  }

  public string get(string key)
  {
    return _get[key];
  }

  public void get(string key, string value)
  {
    _get[key] = value;
  }

  private void SanitizeGlobals()
  {
    var (self, db) = getInstance();
    if (!allowGetArray)
      self.ignore(() => context.Request.QueryString = new QueryString());
    self.ignore(() =>
      SanitizeRequestCollection(context.Request.Query));
    self.ignore(() =>
      SanitizeRequestCollection(context.Request.Form));
    self.ignore(() =>
      SanitizeRequestCollection(context.Request.Cookies));
    self.helper.log_message("debug", "Global POST, GET and COOKIE data sanitized");
  }

  private void SanitizeRequestCollection(IRequestCookieCollection collection)
  {
    foreach (var key in collection.Keys)
    {
      var value = collection[key];
      // collection[key] = CleanInputData(value);
    }
  }

  private void SanitizeRequestCollection(IFormCollection collection)
  {
    foreach (var key in collection.Keys)
    {
      var value = collection[key];
      // collection[key] = CleanInputData(value);
    }
  }

  private void SanitizeRequestCollection(IQueryCollection collection)
  {
    foreach (var key in collection.Keys)
    {
      var value = collection[key];
      //  context.Request.Query =  context.Request.Query.Set(key, CleanInputData(value));
    }
  }

  private string CleanInputData(string str)
  {
    // Implement your cleaning logic (like stripping tags or escaping characters)
    return str ?? string.Empty; // Placeholder: return cleaned data
  }


  public string getPost(string index = null)
  {
    if (_get.Keys.Any(x => x == index))
      return get(index);
    return post().Keys.Contains(index)
      ? post(index)
      : string.Empty;
  }

  public string postGet(string index = null)
  {
    if (_post.Keys.Any(x => x == index))
      return post(index);
    return _get.Keys.Any(x => x == index)
      ? get(index)
      : string.Empty;
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
    var (self, db) = getInstance();
    return self.ip();
  }

  public bool ValidIp(string ip, string type = "")
  {
    return IPAddress.TryParse(ip, out _);
  }

  public string UserAgent(bool xssClean = false)
  {
    return FetchFromHeaders("User-Agent", xssClean);
  }

  public bool IsAjaxRequest()
  {
    return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
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
