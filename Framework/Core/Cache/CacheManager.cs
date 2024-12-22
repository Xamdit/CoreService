using Service.Framework.Helpers;

namespace Service.Framework.Core.Cache;

public class CacheItem
{
  public string Name { get; set; }
  public object Value { get; set; }
}

public class CacheManager(MyInstance self)
{
  public T get<T>(T key)
  {
    return default;
  }

  public void assign(string key, object value)
  {
  }

  public void Init(string uuid)
  {
    var session_folder = "sessions";
    self.helper.create_folder_if_not_exists(session_folder);
    var temp_path = $"{session_folder}/{uuid}.json";
    self.helper.create_json_file(temp_path);
  }

  public bool has(string key)
  {
    return false;
  }
}
