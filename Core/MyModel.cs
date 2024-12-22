using Service.Framework;
using Service.Framework.Library.DataSync;
using Service.Helpers;


namespace Service.Core;

public class MyModel(MyInstance self)
{
  // protected Gateway gateway = new();
  protected DataSyncService gateway = new("https://api.xamdit.com", LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DataSyncService>());
  private int? _staff_user_id = null;
  public int staff_user_id => _staff_user_id ??= self.helper.get_staff_user_id();
  private int? _client_user_id = null;
  public int client_user_id => _client_user_id ??= self.helper.get_client_user_id();

  private bool? _client_logged_in = null;
  public bool client_logged_in => _client_logged_in ??= self.helper.is_client_logged_in();

  private bool? _is_admin = null;
  public bool is_admin => _is_admin ??= self.helper.is_admin();
  public bool? _is_staff_logged_in = null;
  public bool is_staff_logged_in => _is_staff_logged_in ??= self.helper.is_staff_logged_in();
  public MyInstance root = self;
}
