using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Service.Entities;
using Service.Framework.Core.Cache;
using Service.Framework.Core.Engine;
using Service.Framework.Core.Entities;
using Service.Framework.Core.InputSet;
using Service.Libraries.Documents;

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

  public FrameworkContext fw = new();
  public IServiceProvider services { get; set; }

  private NavigationManager _navigation { get; set; }
  public IHttpContextAccessor httpContextAccessor { get; set; }
  public HttpDocument output { get; set; } = new();

  public bool is_logged_in => context.User.Identity.IsAuthenticated;

  public NavigationManager navigation
  {
    get =>
      // if (_navigation == null)
      //   _navigation = services.GetService<NavigationManager>();
      _navigation;
    set => _navigation = value;
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


  public ControllerBase controller { get; set; }
  public Language lang { get; set; }


  public MyInstance()
  {
    helper = new HelperBase();
    ignore(() =>
    {
      if (file_exists("./framework.sqlite")) return;
      create_file_if_not_exists("./framework.sqlite");
      var frameworkContext = new FrameworkContext();
      // frameworkContext.SeedData();
    });
    cache = new CacheManager(this);
    // config = new Config(this).Init();
    input = new MyInput();
    load = new Loader(this, new MyContext());
    library = new LibraryBase();
  }
}
