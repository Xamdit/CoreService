using Newtonsoft.Json;
using Service.Framework.Core.Engine;

namespace Service.Framework.Helpers;

public static class JsonHelper
{
  public static T? Decode<T>(this HelperBase helper, string? json)
  {
    return string.IsNullOrEmpty(json) ? default : JsonConvert.DeserializeObject<T>(json);
  }

  public static string Encode(this HelperBase helper, object? obj)
  {
    return JsonConvert.SerializeObject(obj);
  }

  public static T? convert<T>(this HelperBase helper, object? obj)
  {
    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
  }
}
