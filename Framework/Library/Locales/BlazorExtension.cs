namespace Service.Framework.Library.Locales;

public static class BlazorExtension
{
  public static WebApplicationBuilder AddWithResource<T>(this WebApplicationBuilder builder) where T : class
  {
    builder.Services.AddSingleton((_) =>
      Locale.Current
        .SetNotFoundSymbol("$")
        .SetFallbackLocale("en-US")
        .Init(typeof(T).Assembly)
    );
    return builder;
  }
}
