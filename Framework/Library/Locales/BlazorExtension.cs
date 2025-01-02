namespace Service.Framework.Library.Locales;

public static class BlazorExtension
{
  public static WebApplicationBuilder AddWithResource<T>(this WebApplicationBuilder builder) where T : class
  {
    var self = new MyInstance();
    builder.Services.AddSingleton((_) =>
      Locale.Current
        .SetInstance(self)
        .SetNotFoundSymbol("$")
        .SetFallbackLocale("en-US")
        .Init(typeof(T).Assembly)
    );
    return builder;
  }
}
