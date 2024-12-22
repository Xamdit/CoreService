namespace Service.Helpers.Tags;

public class AppObjectCache
{
  private Dictionary<string, object> _cache = new();

  public T get<T>(string key) where T : class
  {
    var temp = _cache.ContainsKey(key) ? _cache[key] : null;
    return (T)Convert.ChangeType(temp, typeof(T));
  }

  public void add(string key, object value)
  {
    _cache[key] = value;
  }
}
