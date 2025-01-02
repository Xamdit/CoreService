using Service.Framework.Schemas;

namespace Service.Framework.Library.DataStores;

public class TestUnit
{
  [Test]
  public async Task TestCreateItem()
  {
    var store = new DataStore("./configs/config.json");
    var collection = store.GetCollection<Item>();
    collection.AsQueryable()
      .ToList()
      .ForEach(async item =>
      {
        await collection.DeleteOneAsync(item.Name);
        Console.WriteLine("===");
      });
    // var item = new Item { Name = key, Value = items[key] };
    // await collection.InsertOneAsync(item);
    // Console.WriteLine(key);
  }

  [Test]
  public async Task TestOriginal()
  {
    // var store = new DataStore("./Framework/Configuration/data.json");
    // var collection = store.GetCollection<Constant>();
    // var employee = new Constant { Id = 1, Name = "John", Value = "46" };
    // await collection.InsertOneAsync(employee);
    // employee.Name = "John Doe";
    // await collection.UpdateOneAsync(employee.Id, employee);
    // var results = collection.AsQueryable().Where(e => !string.IsNullOrEmpty(e.Value));
    // await store.InsertItemAsync("selected_employee", employee);
    // await store.InsertItemAsync("counter", 1);
    // var counter = store.GetItem<int>("counter");
  }
}
