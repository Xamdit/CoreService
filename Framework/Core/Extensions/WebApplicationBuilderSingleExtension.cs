using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.ResponseCompression;
using Service.Framework.Library.Locales;
using Service.Framework.Library.Themes;
using Service.Framework.Library.Themes.Partials;
using Service.Framework.Middlewares;

namespace Service.Framework.Core.Extensions;

public static class WebApplicationBuilderSingleExtension
{
  public static void SingleSet(this WebApplicationBuilder builder, Action<WebApplication> action)
  {
    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.AddWithResource<Program>();
    // builder.MakeDefault();
    builder.Services.AddSingleton<ITheme, Theme>();
    builder.Services.AddSingleton<IBootstrapBase, BootstrapBase>();
    builder.Services.AddSession(options =>
    {
      options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
      options.Cookie.HttpOnly = true;
      options.Cookie.IsEssential = true;
    });


    builder.Services.AddHttpClient();

    //builder.Services.AddToastService();
    builder.Services.AddResponseCompression(opts => { opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }); });

    // builder.Services.AddSweetAlert2(options => { options.Theme = SweetAlertTheme.Dark; });
    builder.Services.AddSweetAlert2();
    builder.Services.AddBlazoredLocalStorage(config =>
    {
      config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
      config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
      config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
      config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
      config.JsonSerializerOptions.WriteIndented = false;
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<JwtAuthorizationFilter>();

    // Now build the application
    var app = builder.Build();
    // Resolve MyInstance from the built app
    // var self = app.Services.GetRequiredService<MyInstance>();
    // Constants.Instance = self;

    // Initialize MyInstance or perform other setup tasks
    app.UseFramework(); // Call the middleware configuration
    action(app); // Execute the provided action
  }
}
