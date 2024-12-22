namespace Service.Framework.Library.Themes.Partials;

// Core theme class
public class Theme : ITheme
{
  // Theme variables
  private bool _modeSwitchEnabled = true;
  private string _modeDefault = "light";
  private string _direction = "ltr";
  private readonly SortedDictionary<string, SortedDictionary<string, string>> _htmlAttributes = new();
  private readonly SortedDictionary<string, string[]> _htmlClasses = new();

  // Add HTML attributes by scope
  public void AddHtmlAttribute(string scope, string attributeName, string attributeValue)
  {
    SortedDictionary<string, string> attribute = new();
    if (_htmlAttributes.TryGetValue(scope, out var htmlAttribute)) attribute = htmlAttribute;
    attribute[attributeName] = attributeValue;
    _htmlAttributes[scope] = attribute;
  }

  // Add HTML class by scope
  public void AddHtmlClass(string scope, string className)
  {
    var list = new List<string>();
    if (_htmlClasses.TryGetValue(scope, out var @class)) list = @class.ToList();
    list.Add(className);
    _htmlClasses[scope] = list.ToArray();
  }

  // Print HTML attributes for the HTML template
  public string PrintHtmlAttributes(string scope)
  {
    if (!_htmlAttributes.ContainsKey(scope)) return string.Empty;
    var list = _htmlAttributes[scope].Select(attribute => attribute.Key + "=" + attribute.Value).ToList();
    return string.Join(" ", list);
  }

  // Print HTML classes for the HTML template
  public string PrintHtmlClasses(string scope)
  {
    return _htmlClasses.TryGetValue(scope, out var @class)
      ? string.Join(" ", @class)
      : string.Empty;
  }

  // Get SVG icon content
  public string GetSvgIcon(string path, string classNames)
  {
    var svg = File.ReadAllText($"./wwwroot/assets/media/icons/{path}");

    return $"<span class=\"{classNames}\">{svg}</span>";
  }

  // Get keenthemes icon
  public string GetIcon(string iconName, string iconClass = "", string iconType = "")
  {
    var tag = "i";
    var output = "";
    var iconsFinalClass = iconClass == "" ? "" : " " + iconClass;

    if (iconType == "" && ThemeSettings.Config.IconsType != "") iconType = ThemeSettings.Config.IconsType;

    if (iconType == "") iconType = "duotone";

    if (iconType == "duotone")
    {
      if (string.IsNullOrEmpty(iconName)) return output;
      var paths = IconsSettings.Config.TryGetValue(iconName, out var value) ? value : 0;
      output += $"<{tag} class='ki-{iconType} ki-{iconName}{iconsFinalClass}'>";
      for (var i = 0; i < paths; i++) output += $"<span class='path{i + 1}'></span>";
      output += $"</{tag}>";
    }
    else
    {
      output = $"<{tag} class='ki-{iconType} ki-{iconName}{iconsFinalClass}'></{tag}>";
    }

    return output;
  }

  // Set dark mode enabled status
  public void SetModeSwitch(bool flag)
  {
    _modeSwitchEnabled = flag;
  }

  // Check dark mode status
  public bool IsModeSwitchEnabled()
  {
    return _modeSwitchEnabled;
  }

  // Set the mode to dark or light
  public void SetModeDefault(string flag)
  {
    _modeDefault = flag;
  }

  // Get current mode
  public string GetModeDefault()
  {
    return _modeDefault;
  }

  // Set style direction
  public void SetDirection(string direction)
  {
    _direction = direction;
  }

  // Get style direction
  public string GetDirection()
  {
    return _direction;
  }

  // Check if style direction is RTL
  public bool IsRtlDirection()
  {
    return _direction.ToLower() == "rtl";
  }

  public string GetAssetPath(string path)
  {
    return $"/{ThemeSettings.Config.AssetsDir}{path}";
  }

  // Extend CSS file name with RTL
  public string ExtendCssFilename(string path)
  {
    if (IsRtlDirection()) path = path.Replace(".css", ".rtl.css");

    return path;
  }

  // Include favicon from settings
  public string GetFavicon()
  {
    var output = GetAssetPath(ThemeSettings.Config.Assets.Favicon);
    return output;
  }

  // Include the fonts from settings
  public List<string> GetFonts()
  {
    var output = ThemeSettings.Config.Assets.Fonts;
    output.ToList().ForEach(Console.WriteLine);
    return output;
  }

  // Get the global assets
  public List<string> GetGlobalAssets(string type)
  {
    var files = type == "Css"
      ? ThemeSettings.Config.Assets.Css
      : ThemeSettings.Config.Assets.Js;
    var output = files.Select(file => type == "Css" ? GetAssetPath(ExtendCssFilename(file)) : GetAssetPath(file)).ToList();
    output.ToList().ForEach(Console.WriteLine);
    return output;
  }

  public string GetAttributeValue(string scope, string attributeName)
  {
    if (!_htmlAttributes.ContainsKey(scope)) return string.Empty;
    return _htmlAttributes[scope].ContainsKey(attributeName) ? _htmlAttributes[scope][attributeName] : string.Empty;
  }
}
