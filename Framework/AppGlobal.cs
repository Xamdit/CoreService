using Service.Entities;
using Service.Framework.Library;

namespace Service.Framework;

public static class AppGlobal
{
  public static WebApplication? Instance { get; private set; }

  public static MyInstance self =>
    (Instance != null
      ? Instance.Services.GetService<MyInstance>()
      : new MyInstance())!;

  public static MyApp? my_app { get; private set; }


  public static void SetMyApp(MyApp _my_app)
  {
    my_app = _my_app;
  }

  public static MyContext MyContext =>
    (Instance != null
      ? Instance.Services.GetService<MyContext>()
      : new MyContext())!;


  public static void SetInstance(WebApplication app)
  {
    Instance = app;
  }
}
