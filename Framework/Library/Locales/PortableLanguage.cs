namespace Service.Framework.Library.Locales;

public class PortableLanguage
{
  public string Locale { get; set; }
  public string DisplayName { get; set; }

  public override string ToString()
  {
    return DisplayName;
  }
}
