using System.Dynamic;

namespace Service.Framework.Core.AppHook;

public class DynamicWrapper
{
  private readonly Dictionary<string, object> _properties = new();

  public DynamicWrapper(params object[] values)
  {
    for (var i = 0; i < values.Length; i++) _properties[$"Value{i + 1}"] = values[i];
  }

  public object this[string key] => _properties.ContainsKey(key) ? _properties[key] : null;

  public override string ToString()
  {
    return string.Join(", ", _properties.Select(kv => $"{kv.Key}: {kv.Value}"));
  }

  // Dynamic property access
  public dynamic ToDynamic()
  {
    var expando = new ExpandoObject() as IDictionary<string, object>;
    foreach (var kvp in _properties) expando[kvp.Key] = kvp.Value;
    return expando;
  }
}
