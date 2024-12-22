using Service.Framework.Library.Themes.Partials;

namespace Service.Framework.Library.Themes;

public interface IBootstrapBase
{
  void InitThemeMode();

  void InitThemeDirection();

  void InitRtl();

  void InitLayout();

  void Init(ITheme theme);
}
