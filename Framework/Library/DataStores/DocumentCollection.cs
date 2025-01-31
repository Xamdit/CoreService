using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Service.Framework.Library.DataStores;

public class DocumentCollection<T>(Func<string, Func<List<T>, bool>, bool, Task<bool>> commit, Lazy<List<T>> data, string path, string idField,
    Func<T, T> insertConvert, Func<T> createNewInstance)
  : IDocumentCollection<T>
{
  public int Count => data.Value.Count;

  public IEnumerable<T> AsQueryable()
  {
    return data.Value.AsQueryable();
  }

  public IEnumerable<T> Find(Predicate<T> query)
  {
    return data.Value.Where(t => query(t));
  }

  public IEnumerable<T> Find(string text, bool caseSensitive = false)
  {
    return data.Value.Where(t => ObjectExtensions.FullTextSearch(t, text, caseSensitive));
  }

  public dynamic GetNextIdValue()
  {
    return GetNextIdValue(data.Value);
  }

  public bool InsertOne(T item)
  {
    ExecuteLocked(UpdateAction, data.Value);

    return commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var itemToInsert = GetItemToInsert(GetNextIdValue(data, item), item, insertConvert);
      data.Add(itemToInsert);
      return true;
    }
  }

  public async Task<bool> InsertOneAsync(T item)
  {
    ExecuteLocked(UpdateAction, data.Value);

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var itemToInsert = GetItemToInsert(GetNextIdValue(data, item), item, insertConvert);
      data.Add(itemToInsert);
      return true;
    }
  }

  public bool InsertMany(IEnumerable<T> items)
  {
    ExecuteLocked(UpdateAction, data.Value);

    return commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      foreach (var item in items)
      {
        var itemToInsert = GetItemToInsert(GetNextIdValue(data, item), item, insertConvert);
        data.Add(itemToInsert);
      }

      return true;
    }
  }

  public async Task<bool> InsertManyAsync(IEnumerable<T> items)
  {
    ExecuteLocked(UpdateAction, data.Value);

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      foreach (var item in items)
      {
        var itemToInsert = GetItemToInsert(GetNextIdValue(data, item), item, insertConvert);
        data.Add(itemToInsert);
      }

      return true;
    }
  }

  public bool ReplaceOne(Predicate<T> filter, T item, bool upsert = false)
  {
    return ExecuteLocked(UpdateAction, data.Value) && commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e)).ToList();

      if (!matches.Any())
      {
        if (!upsert)
          return false;

        var newItem = createNewInstance();
        ObjectExtensions.CopyProperties(item, newItem);
        data.Add(insertConvert(newItem));
        return true;
      }

      var index = data.IndexOf(matches.First());
      data[index] = item;

      return true;
    }
  }

  public bool ReplaceOne(dynamic id, T item, bool upsert = false)
  {
    return ReplaceOne(GetFilterPredicate(id), item, upsert);
  }

  public bool ReplaceMany(Predicate<T> filter, T item)
  {
    return ExecuteLocked(UpdateAction, data.Value) && commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e)).ToList();

      if (!matches.Any())
        return false;

      foreach (var index in matches.ToList().Select(match => data.IndexOf(match))) data[index] = item;

      return true;
    }
  }

  public async Task<bool> ReplaceOneAsync(Predicate<T> filter, T item, bool upsert = false)
  {
    if (!ExecuteLocked(UpdateAction, data.Value))
      return false;

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e)).ToList();

      if (!matches.Any())
      {
        if (!upsert)
          return false;

        var newItem = createNewInstance();
        ObjectExtensions.CopyProperties(item, newItem);
        data.Add(insertConvert(newItem));
        return true;
      }

      var index = data.IndexOf(matches.First());
      data[index] = item;

      return true;
    }
  }

  public Task<bool> ReplaceOneAsync(dynamic id, T item, bool upsert = false)
  {
    return ReplaceOneAsync(GetFilterPredicate(id), item, upsert);
  }

  public async Task<bool> ReplaceManyAsync(Predicate<T> filter, T item)
  {
    if (!ExecuteLocked(UpdateAction, data.Value))
      return false;

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e));

      if (!matches.Any())
        return false;

      foreach (var index in matches.ToList().Select(match => data.IndexOf(match))) data[index] = item;

      return true;
    }
  }

  public bool UpdateOne(Predicate<T> filter, dynamic item)
  {
    return ExecuteLocked(UpdateAction, data.Value) && commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e));

      if (!matches.Any())
        return false;

      var toUpdate = matches.First();
      ObjectExtensions.CopyProperties(item, toUpdate);

      return true;
    }
  }

  public bool UpdateOne(dynamic id, dynamic item)
  {
    return UpdateOne(GetFilterPredicate(id), item);
  }

  public async Task<bool> UpdateOneAsync(Predicate<T> filter, dynamic item)
  {
    if (!ExecuteLocked(UpdateAction, data.Value))
      return false;
    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e));

      if (!matches.Any())
        return false;

      var toUpdate = matches.First();
      ObjectExtensions.CopyProperties(item, toUpdate);

      return true;
    }
  }

  public Task<bool> UpdateOneAsync(dynamic id, dynamic item)
  {
    return UpdateOneAsync(GetFilterPredicate(id), item);
  }

  public bool UpdateMany(Predicate<T> filter, dynamic item)
  {
    return ExecuteLocked(UpdateAction, data.Value) && commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e));

      if (!matches.Any())
        return false;

      foreach (var toUpdate in matches) ObjectExtensions.CopyProperties(item, toUpdate);

      return true;
    }
  }

  public async Task<bool> UpdateManyAsync(Predicate<T> filter, dynamic item)
  {
    if (!ExecuteLocked(UpdateAction, data.Value))
      return false;

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var matches = data.Where(e => filter(e));

      if (!matches.Any())
        return false;

      foreach (var toUpdate in matches) ObjectExtensions.CopyProperties(item, toUpdate);

      return true;
    }
  }

  public bool DeleteOne(Predicate<T> filter)
  {
    return ExecuteLocked(UpdateAction, data.Value) && commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var remove = data.FirstOrDefault(e => filter(e));

      return remove != null && data.Remove(remove);
    }
  }

  public bool DeleteOne(dynamic id)
  {
    return DeleteOne(GetFilterPredicate(id));
  }

  public async Task<bool> DeleteOneAsync(Predicate<T> filter)
  {
    if (!ExecuteLocked(UpdateAction, data.Value))
      return false;

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var remove = data.FirstOrDefault(e => filter(e));

      return remove != null && data.Remove(remove);
    }
  }

  public Task<bool> DeleteOneAsync(dynamic id)
  {
    return DeleteOneAsync(GetFilterPredicate(id));
  }

  public bool DeleteMany(Predicate<T> filter)
  {
    return ExecuteLocked(UpdateAction, data.Value) && commit(path, UpdateAction, false).Result;

    bool UpdateAction(List<T> data)
    {
      var removed = data.RemoveAll(filter);
      return removed > 0;
    }
  }

  public async Task<bool> DeleteManyAsync(Predicate<T> filter)
  {
    if (!ExecuteLocked(UpdateAction, data.Value))
      return false;

    return await commit(path, UpdateAction, true).ConfigureAwait(false);

    bool UpdateAction(List<T> data)
    {
      var removed = data.RemoveAll(filter);
      return removed > 0;
    }
  }

  private bool ExecuteLocked(Func<List<T>, bool> func, List<T> data)
  {
    lock (data)
    {
      return func(data);
    }
  }

  private dynamic GetNextIdValue(List<T> data, T item = default)
  {
    if (!data.Any())
    {
      if (item == null) return ObjectExtensions.GetDefaultValue<T>(idField);
      var primaryKeyValue = GetFieldValue(item, idField);

      if (primaryKeyValue == null) return ObjectExtensions.GetDefaultValue<T>(idField);
      // Without casting dynamic will return Int64 for int
      if (primaryKeyValue is long)
        return (int)primaryKeyValue;

      return primaryKeyValue;
    }

    var lastItem = data.Last();

    var keyValue = GetFieldValue(lastItem, idField);

    if (keyValue == null)
      return null;

    if (keyValue is long)
      return (int)keyValue + 1;

    return ParseNextIntegerToKeyValue(keyValue.ToString());
  }

  private dynamic? GetFieldValue(T item, string fieldName)
  {
    var expando = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(item), new ExpandoObjectConverter());
    // Problem here is if we have typed data with upper camel case properties but lower camel case in JSON, so need to use OrdinalIgnoreCase string comparer
    var expandoAsIgnoreCase = new Dictionary<string, dynamic>(expando, StringComparer.OrdinalIgnoreCase);

    return !expandoAsIgnoreCase.ContainsKey(fieldName)
      ? null
      : expandoAsIgnoreCase[fieldName];
  }

  private string ParseNextIntegerToKeyValue(string? input)
  {
    var nextInt = 0;

    if (input == null)
      return $"{nextInt}";

    var chars = input.ToArray().Reverse().TakeWhile(char.IsNumber).Reverse().ToArray();

    if (!chars.Any())
      return $"{input}{nextInt}";

    input = input[..^chars.Count()];

    if (int.TryParse(new string(chars), out nextInt))
      nextInt += 1;

    return $"{input}{nextInt}";
  }

  private T GetItemToInsert(dynamic insertId, T item, Func<T, T> insertConvert)
  {
    if (insertId == null)
      return insertConvert(item);

    // If item to be inserted is an anonymous type and it is missing the id-field, then add new id-field
    // If it has an id-field, then we trust that user know what value he wants to insert
    if (ObjectExtensions.IsAnonymousType(item) && ObjectExtensions.HasField(item, idField) == false)
    {
      var toReturn = insertConvert(item);
      ObjectExtensions.AddDataToField(toReturn, idField, insertId);
      return toReturn;
    }

    ObjectExtensions.AddDataToField(item, idField, insertId);
    return insertConvert(item);
  }

  private Predicate<T> GetFilterPredicate(dynamic id)
  {
    return e => ObjectExtensions.GetFieldValue(e, idField) == id;
  }
}
