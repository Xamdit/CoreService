using Newtonsoft.Json;
using Service.Framework.Core.Entities;
using Service.Framework.Entities;
using Service.Framework.Library.DataStores;

namespace Service.Framework.Core.Engine;

public class MyConfig(MyInstance self)
{
  private bool IsLoad = false;

  private IDocumentCollection<ConfigItem> context
  {
    get
    {
      var store = new DataStore("./configs/config.json");
      var collection = store.GetCollection<ConfigItem>();
      return collection;
    }
  }


  public MyConfig Init()
  {
    return this;
  }

  public bool Load(string file = "", bool useSections = false, bool failGracefully = false)
  {
    file = string.IsNullOrEmpty(file) ? "config" : file.Replace(".php", "");
    var loaded = false;
    if (loaded) return true;
    if (failGracefully) return false;
    throw new Exception($"The configuration file {file}.json does not exist.");
  }

  public T item<T>(string item, string index = "")
  {
    var row = context.AsQueryable().FirstOrDefault(x => x.Name == item);
    if (row == null)
      return (T)Convert.ChangeType(null, typeof(T))!;
    var value = $"{row.Value}";
    return (T)Convert.ChangeType(value, typeof(T));
  }

  public string SlashItem(string item)
  {
    var row = context.AsQueryable().FirstOrDefault(x => x.Name == item);
    if (row == null) return string.Empty;
    var value = $"{row.Value}";
    return !string.IsNullOrWhiteSpace(value) ? value.TrimEnd('/') + "/" : string.Empty;
  }

  public string SiteUrl(string uri = "", string protocol = null)
  {
    return BuildUrl(uri, protocol, true);
  }

  public string BaseUrl(string uri = "", string protocol = null)
  {
    return BuildUrl(uri, protocol, false);
  }

  private string BuildUrl(string uri, string protocol, bool includeIndexPage)
  {
    var baseUrl = SlashItem("base_url");
    if (!string.IsNullOrEmpty(protocol))
      baseUrl = string.IsNullOrEmpty(protocol) ? baseUrl[baseUrl.IndexOf("//", StringComparison.Ordinal)..] : protocol + baseUrl.Substring(baseUrl.IndexOf("://"));

    if (string.IsNullOrEmpty(uri)) return baseUrl + (includeIndexPage ? item<string>("index_page") : string.Empty);

    uri = UriString(uri);
    var suffix = context.AsQueryable().FirstOrDefault(x => x.Name == "url_suffix");
    if (suffix == null) return string.Empty;
    var indexPage = includeIndexPage ? SlashItem("index_page") : string.Empty;
    // uri = item("enable_query_strings") is false
    //   ? !string.IsNullOrEmpty(suffix.Value)
    //     ? uri.Contains("?") ? uri[..uri.IndexOf('?')] + suffix + uri[uri.IndexOf('?')..] : uri + suffix
    //     : uri
    //   : !uri.Contains("?")
    //     ? "?" + uri
    //     : uri;

    return baseUrl + indexPage + uri;
  }

  private string UriString(string uri)
  {
    return item<bool>("enable_query_strings") is false
      ? string.Join("/", uri.Split('/').Where(segment => !string.IsNullOrWhiteSpace(segment)))
      : uri;
  }

  public void assign(string item, object value)
  {
    var row = context.AsQueryable().FirstOrDefault(x => x.Name == item);
    if (row == null)
      context.InsertOne(new ConfigItem
      {
        Group = "default",
        Name = item,
        Value = value.ToString()
      });
    else row.Value = value.ToString();
  }

  private Dictionary<string, object> LoadConfigFromFile(string filePath)
  {
    var json = File.ReadAllText(filePath);
    return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
  }

  public string get(string key)
  {
    return string.Empty;
  }

  public T get<T>(string key, T defaultValue, bool create_if_not_exists = false)
  {
    var db = new FrameworkContext();
    if (!db.Configs.Any(x => x.Name == key) && create_if_not_exists)
    {
      db.Configs.Add(new Config()
      {
        Name = key,
        Value = Convert.ToString(defaultValue)
      });
      db.SaveChanges();
    }

    var row = db.Configs.First(x => x.Name == key);
    return (T)Convert.ChangeType(row.Value, typeof(T))!;
  }
}
