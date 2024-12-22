namespace Service.Framework.Library.Themes.Partials;

public class IconsSettings
{
  public static SortedDictionary<string, int> Config { get; set; } = new();

  public static void Init(IConfiguration configuration)
  {
    var temp = configuration.GetSection("duotone-paths");
    Config = temp.Get<SortedDictionary<string, int>>() ?? Config;
  }
}
