namespace Service.Framework.Library.DataStores;

public interface IDataStore : IDisposable
{
  bool IsUpdating { get; }
  IDocumentCollection<dynamic> GetCollection(string name);
  IDocumentCollection<T> GetCollection<T>(string name = null) where T : class;
  IDictionary<string, ValueType> GetKeys(ValueType? typeToGet = null);
  void UpdateAll(string jsonData);
  void Reload();
  T GetItem<T>(string key);
  dynamic? GetItem(string key);
  bool InsertItem<T>(string key, T item);
  Task<bool> InsertItemAsync<T>(string key, T item);

  /// <returns>true if item found for replacement</returns>
  bool ReplaceItem<T>(string key, T item, bool upsert = false);

  /// <returns>true if item found for replacement</returns>
  Task<bool> ReplaceItemAsync<T>(string key, T item, bool upsert = false);

  bool UpdateItem(string key, dynamic? item);
  Task<bool> UpdateItemAsync(string key, dynamic? item);
  bool DeleteItem(string key);
  Task<bool> DeleteItemAsync(string key);
}

public enum ValueType
{
  Collection,
  Item
}
