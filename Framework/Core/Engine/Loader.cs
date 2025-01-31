using DotLiquid;
using Microsoft.AspNetCore.Components;
using Service.Entities;
using Service.Framework.Helpers.Context;
using File = System.IO.File;
using Template = DotLiquid.Template;

namespace Service.Framework.Core.Engine;

public class Loader(MyInstance self, MyContext db)
{
  public MarkupString markup(string route, object data = default)
  {
    var theme = self.config.get<string>("theme", "defaults");
    var path = Path.Combine(Directory.GetCurrentDirectory(), theme, route);
    var content = File.ReadAllText(path);
    var template = Template.Parse(content);
    var output = template.Render(Hash.FromAnonymousObject(data));
    return new MarkupString(output);
  }

  public string view(string route, object data = default)
  {
    var theme = db.config("theme", "defaults");
    var path = Path.Combine(
      $"templates/{theme}/{route}"
    );
    if (!file_exists(path)) return string.Empty;
    if (!route.Contains("."))
      path = $"{path}.html";
    if (!File.Exists(path)) File.Create(path);

    return string.Empty;
  }

  public object controller()
  {
    // var serviceProvider = new ServiceProvider(); // Your IServiceProvider instance
    // var apiInvoker = new ApiInvoker(serviceProvider);
    // var result = apiInvoker.CallRoute("/api/label");
    //
    // Console.WriteLine(result);
    return null;
  }
}
