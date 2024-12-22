namespace Service.Framework.Library.DataStores;

public interface IDocumentCollection<T>
{
  IEnumerable<T> AsQueryable();
  IEnumerable<T> Find(Predicate<T> query);
  IEnumerable<T> Find(string text, bool caseSensitive = false);
  dynamic GetNextIdValue();
  bool InsertOne(T item);
  Task<bool> InsertOneAsync(T item);
  bool InsertMany(IEnumerable<T> items);
  Task<bool> InsertManyAsync(IEnumerable<T> items);
  bool ReplaceOne(Predicate<T> filter, T item, bool upsert = false);
  bool ReplaceOne(dynamic id, T item, bool upsert = false);
  Task<bool> ReplaceOneAsync(Predicate<T> filter, T item, bool upsert = false);
  Task<bool> ReplaceOneAsync(dynamic id, T item, bool upsert = false);
  bool ReplaceMany(Predicate<T> filter, T item);
  Task<bool> ReplaceManyAsync(Predicate<T> filter, T item);
  bool UpdateOne(Predicate<T> filter, dynamic item);
  bool UpdateOne(dynamic id, dynamic item);
  Task<bool> UpdateOneAsync(Predicate<T> filter, dynamic item);
  Task<bool> UpdateOneAsync(dynamic id, dynamic item);
  bool UpdateMany(Predicate<T> filter, dynamic item);
  Task<bool> UpdateManyAsync(Predicate<T> filter, dynamic item);
  bool DeleteOne(Predicate<T> filter);
  bool DeleteOne(dynamic id);
  Task<bool> DeleteOneAsync(Predicate<T> filter);
  Task<bool> DeleteOneAsync(dynamic id);
  bool DeleteMany(Predicate<T> filter);
  Task<bool> DeleteManyAsync(Predicate<T> filter);
  int Count { get; }
}
