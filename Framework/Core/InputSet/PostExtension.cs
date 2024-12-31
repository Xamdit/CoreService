using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Service.Framework.Core.InputSet;

public static class PostExtension
{
  private static Dictionary<string, StringValues> dataset = new();

  public static Dictionary<string, StringValues> post()
  {
    return dataset;
  }


  public static StringValues post(this MyInput input, string name)
  {
    if (dataset.ContainsKey(name)) return dataset[name];
    var keys = input?.context?.Request?.Form?.Keys?.ToList();
    if (keys == null) return dataset.ContainsKey(name) ? dataset[name] : StringValues.Empty;
    foreach (var key in keys)
    {
      var queryValue = input.context.Request.Query[key];
      if (!dataset.ContainsKey(key)) dataset.Add(key, queryValue);
    }

    return dataset.ContainsKey(name) ? dataset[name] : StringValues.Empty;
  }

  public static T? post<T>(this MyInput input) where T : class
  {
    var _dataset = new Dictionary<string, object>();
    var keys = input?.context?.Request?.Form?.Keys?.ToList();
    foreach (var k in keys)
    {
      var value = Convert.ToString(input?.context?.Request?.Form[k]);
      _dataset.Add(k, value);
    }

    var jsonString = JsonConvert.SerializeObject(_dataset);
    var output = JsonConvert.DeserializeObject<T>(jsonString);
    return output;
  }

  public static T? post<T>(this MyInput input, string key)
  {
    var _dataset = new Dictionary<string, object>();
    var keys = input?.context?.Request?.Form?.Keys?.ToList();


    if (keys == null || !keys.Contains(key)) return default;


    foreach (var k in keys)
    {
      var value = Convert.ToString(input?.context?.Request?.Form[k]);
      _dataset.Add(k, value);
    }

    if (!_dataset.TryGetValue(key, out var rawValue)) return default;
    try
    {
      return (T)Convert.ChangeType(rawValue, typeof(T));
    }
    catch
    {
      // Handle conversion error
      return default;
    }

    return default;
  }


  public static bool post_has(this MyInput input, string name)
  {
    var keys = input?.context?.Request?.Form?.Keys?.ToList();
    return keys.Contains(name);
  }
}
