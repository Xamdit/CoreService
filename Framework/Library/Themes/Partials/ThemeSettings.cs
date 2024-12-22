namespace Service.Framework.Library.Themes.Partials;

public class ThemeSettings
{
  public static ThemeBase Config = new();

  public static void Init(IConfiguration configuration)
  {
    Config = configuration.GetSection("Theme").Get<ThemeBase>() ?? Config;
  }
}
