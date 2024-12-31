using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Framework.Core.AppHook;
using Service.Framework.Core.Cache;
using Service.Framework.Core.Engine;
using Service.Framework.Core.Entities;
using Service.Framework.Core.Extensions;
using Service.Framework.Core.InputSet;
using Service.Framework.Entities;
using Service.Framework.Helpers;
using Service.Libraries.Documents;
using Session = Service.Entities.Session;

namespace Service.Framework;

public class MyInstance : IMyInstance
{
  public MyConfig config { get; set; }
  public MyInput input { get; set; }
  public Loader load { get; set; }
  public JavascriptBase js = new();
  private HttpContext _context = null;
  public CacheManager cache { get; set; }
  public HelperBase helper = new();
  public LibraryBase library { get; set; }
  public ModelBase model { get; set; }
  public FrameworkContext fw = new();
  public IServiceProvider services { get; set; }

  private NavigationManager _navigation { get; set; }
  public IHttpContextAccessor httpContextAccessor { get; set; }
  public HttpDocument output { get; set; } = new();

  public DbSet<Session> session
  {
    get
    {
      var (self, db) = getInstance();
      return db.Sessions;
    }
  }

  public NavigationManager navigation
  {
    get =>
      // if (_navigation == null)
      //   _navigation = services.GetService<NavigationManager>();
      _navigation;
    set => _navigation = value;
  }

  public Hooks hooks
  {
    get
    {
      var output = new Hooks(this);
      return output;
    }
  }

  public HttpContext context
  {
    get => _context;
    set
    {
      _context = value; // Fix: Set the backing field
      input.Init(value);
    }
  }

  private static MyInstance _instance;

  public ControllerBase controller { get; set; }
  public Language lang { get; set; }

  public static MyInstance Instance => _instance ??= new MyInstance();


  private MyInstance()
  {
    helper = new HelperBase();
    this.ignore(() =>
    {
      if (helper.file_exists("./framework.sqlite")) return;
      helper.create_file_if_not_exists("./framework.sqlite");
      var frameworkContext = new FrameworkContext();
      // frameworkContext.SeedData();
    });

    helper.log_message("info", "MyInstance Class Initialized");
    cache = new CacheManager(this);
    // config = new Config(this).Init();
    input = new MyInput();
    load = new Loader(this);
    library = new LibraryBase();
    model = new ModelBase();
  }
}
