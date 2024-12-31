using Newtonsoft.Json;

namespace Service.Framework.Helpers;

public static class GlobalHelper
{
  public static T? convert<T>(this object obj)
  {
    var jsonString = JsonConvert.SerializeObject(obj);
    var dict = JsonConvert.DeserializeObject<T>(jsonString);
    return dict;
  }
}
