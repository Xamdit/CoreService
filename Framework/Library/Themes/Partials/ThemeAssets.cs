namespace Service.Framework.Library.Themes.Partials;

public class ThemeAssets
{
  public string Favicon { get; set; } = string.Empty;

  public List<string> Fonts { get; set; } = new();

  public List<string> Css { get; set; } = new();

  public List<string> Js { get; set; } = new();
}
