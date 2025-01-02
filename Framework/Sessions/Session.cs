using Service.Framework.Library.DataStores;
using Service.Framework.Schemas;

namespace Service.Framework.Sessions;

public class Session
{
  private string session_dir = "sessions";
  private readonly IDocumentCollection<Item> collection;


  public Session(MyInstance instance, string session_id)
  {
    var store = new DataStore($"{session_dir}/{session_id}.json");
    collection = store.GetCollection<Item>();
  }

  public bool set(string key, object value)
  {
    if (collection.AsQueryable().Any(x => x.Name == key))
    {
      var row = collection.AsQueryable().First(x => x.Name == key);
      row.Value = value;
      collection.UpdateOneAsync(row.Id, row);
      return true;
    }

    var item = new Item { Name = key, Value = value };
    collection.InsertOneAsync(item);
    return true;
  }

  public void delete(string name)
  {
    collection.DeleteOneAsync(e => e.Name == name);
  }

  public object? get(string name)
  {
    var output = collection.AsQueryable().FirstOrDefault(e => e.Name == name);
    return output?.Value;
  }

  public T? get<T>(string name)
  {
    var output = collection.AsQueryable().FirstOrDefault(e => e.Name == name);
    return (T?)Convert.ChangeType(output?.Value, typeof(T));
  }

  public void unset_userdata(string key)
  {
  }

  public bool has_userdata(string key)
  {
    return false;
  }

  public void set_userdata(string key, object value)
  {
  }

  public void set_userdata(object args)
  {
  }

  public string? user_data(string key)
  {
    return string.Empty;
  }

  public string get_string(string key)
  {
    return key;
  }

  public string get_userdata(string key)
  {
    return key;
  }
}
