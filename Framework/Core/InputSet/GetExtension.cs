using Microsoft.Extensions.Primitives;

namespace Service.Framework.Core.InputSet;

public static class GetExtension
{
  private static Dictionary<string, StringValues> dataset = new();

  public static Dictionary<string, StringValues> get()
  {
    return dataset;
  }

  public static string get(this MyInput input, string name)
  {
    if (dataset.ContainsKey(name)) return dataset[name];
    var keys = input?.context?.Request?.Query?.Keys?.ToList();
    if (keys == null) return dataset.ContainsKey(name) ? dataset[name] : StringValues.Empty;
    foreach (var key in keys)
    {
      var queryValue = input.context.Request.Query[key];
      if (!dataset.ContainsKey(key)) dataset.Add(key, queryValue);
    }

    return dataset.ContainsKey(name) ? dataset[name] : StringValues.Empty;
  }

  public static bool get_has(this MyInput input, string name)
  {
    var keys = input?.context?.Request?.Query?.Keys?.ToList();
    return keys.Contains(name);
  }
}
