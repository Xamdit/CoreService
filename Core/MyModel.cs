using Service.Entities;
using Service.Framework;
using Service.Framework.Library.DataSync;
using Service.Helpers;


namespace Service.Core;

public class MyModel(MyInstance self, MyContext db)
{
  // protected Gateway gateway = new();
  protected DataSyncService gateway = new("https://api.xamdit.com", LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DataSyncService>());



  public (MyInstance self, MyContext db) getInstance() => (self, db);
}
