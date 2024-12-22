using Service.Framework.Library.Themes.Partials;

namespace Service.Framework.Library.Themes;

public class BootstrapBase : IBootstrapBase
{
  private ITheme _theme = default!;

  // Init theme mode option from settings
  public void InitThemeMode()
  {
    _theme.SetModeSwitch(ThemeSettings.Config.ModeSwitchEnabled);
    _theme.SetModeDefault(ThemeSettings.Config.ModeDefault);
  }

  // Init theme direction option (RTL or LTR) from settings
  public void InitThemeDirection()
  {
    _theme.SetDirection(ThemeSettings.Config.Direction);
  }

  // Init RTL html attributes by checking if RTL is enabled.
  // This function is being called for the html tag
  public void InitRtl()
  {
    if (!_theme.IsRtlDirection()) return;
    _theme.AddHtmlAttribute("html", "direction", "rtl");
    _theme.AddHtmlAttribute("html", "dir", "rtl");
    _theme.AddHtmlAttribute("html", "style", "direction: rtl");
  }

  // Init layout
  public void InitLayout()
  {
    _theme.AddHtmlAttribute("body", "id", "kt_app_body");
    _theme.AddHtmlAttribute("body", "data-kt-app-page-loading", "on");
  }

  // Global theme initializer
  public void Init(ITheme theme)
  {
    _theme = theme;
    InitThemeMode();
    InitThemeDirection();
    InitRtl();
    InitLayout();
  }
}
