namespace Service.Framework.Library.Themes.Partials;

public class ThemeGlobalMethod
{
  public static string get_asset_path(string path)
  {
    // return $"/{ThemeSettings.Config.AssetsDir}{path}";
    return $"/assets/{path}";
  }
}
