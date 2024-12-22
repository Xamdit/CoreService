using Service.Core.Middlewares;
using Service.Framework.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.SingleSet((app) =>
{
  app.UseStaticFiles();
  app.UseMiddleware<WebMiddleware>();
  app.MapBlazorHub();
  app.MapFallbackToPage("/_Host");
  app.Run();
});
