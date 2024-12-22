using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Service.Core.Extensions;

public static class NavigationManagerExtension
{
  public static string GetSection(this NavigationManager navigationManager)
  {
    var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
    var segments = uri.Segments;
    var userType = segments.Length > 1 ? segments[1].Trim('/') : string.Empty;
    return userType;
  }

  public static string base_url(this NavigationManager navigationManager, string route = "", object args = null)
  {
    var fullUrl = navigationManager.Uri;
    var uri = new Uri(fullUrl);
    var baseUrl = $"{uri.Scheme}://{uri.Host}";
    if (!uri.IsDefaultPort) baseUrl += $":{uri.Port}";
    if (!string.IsNullOrEmpty(route)) baseUrl += $"/{route}";
    if (args == null) return baseUrl;
    var jsonString = JsonConvert.SerializeObject(args);
    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
    if (json == null) return baseUrl;
    var values = json.Select(x => $"{x.Key}={x.Value}").ToList();
    var queryParams = string.Join("&", values);
    baseUrl += "?" + queryParams;
    return baseUrl;
  }

  public static string admin_url(this NavigationManager navigationManager, string url = "", object args = null)
  {
    var output = string.Empty;
    if (string.IsNullOrEmpty(url)) return navigationManager.base_url("admin", args);
    var items = url.Split("/").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList();
    items.Insert(0, "admin");
    output = navigationManager.base_url(string.Join("/", items), args);
    return output;
  }

  public static string client_url(this NavigationManager navigationManager, string url, object args = null)
  {
    var output = string.Empty;
    if (string.IsNullOrEmpty(url)) return navigationManager.base_url("client", args);
    var items = url.Split("/").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList();
    items.Insert(0, "client");
    output = navigationManager.base_url(string.Join("/", items), args);
    return output;
  }
}
