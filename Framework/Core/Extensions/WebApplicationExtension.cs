using Service.Framework.Library.Themes.Partials;

namespace Service.Framework.Core.Extensions;

public static class WebApplicationExtension
{
  public static void UseFramework(this WebApplication app)
  {
    ThemeSettings.Init(new ConfigurationBuilder()
      .AddJsonFile("configs/themes.json")
      .Build());

    IconsSettings.Init(new ConfigurationBuilder()
      .AddJsonFile("configs/icons.json")
      .Build());

    if (!app.Environment.IsDevelopment())
    {
      app.UseExceptionHandler("/Error", true);
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.UseRouting();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();
    app.MapControllers();
    // app.UseSession(); // Enable session middleware
    // app.UseMiddleware<FrameworkMiddleware>();
    // app.UseMiddleware<CacheMiddleware>();
    // app.UseMiddleware<NotFoundMiddleware>();
  }
}
