namespace Service.Framework.Library.Themes.Partials;

// Base type class for theme settings
public class ThemeBase
{
  public string LayoutDir { get; set; } = string.Empty;

  public string Direction { get; set; } = string.Empty;

  public bool ModeSwitchEnabled { get; set; } = false;

  public string ModeDefault { get; set; } = string.Empty;

  public string AssetsDir { get; set; } = string.Empty;

  public string IconsType { get; set; } = string.Empty;

  public ThemeAssets Assets { get; set; } = new();

  public SortedDictionary<string, SortedDictionary<string, string[]>> Vendors { get; set; } = new();
}
