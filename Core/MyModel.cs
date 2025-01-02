using Service.Entities;
using Service.Framework;
using Service.Framework.Library.DataSync;
using Service.Helpers;


namespace Service.Core;

public class MyModel(MyInstance self, MyContext db)
{
  // protected Gateway gateway = new();
  protected DataSyncService gateway = new("https://api.xamdit.com", LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DataSyncService>());
  private int? _staff_user_id = null;
  public int staff_user_id => _staff_user_id ??= db.get_staff_user_id();
  private int? _client_user_id = null;
  public int client_user_id => _client_user_id ??= db.get_client_user_id();


  public (MyInstance, MyContext ) getInstance()
  {
    return (self, db);
  }
}
