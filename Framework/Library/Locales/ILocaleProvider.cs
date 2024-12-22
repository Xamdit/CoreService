namespace Service.Framework.Library.Locales;

public interface ILocaleProvider : IDisposable
{
  IEnumerable<Tuple<string, string>> GetAvailableLocales();
  Stream GetLocaleStream(string locale);
  ILocaleProvider SetLogger(Action<string> logger);
  ILocaleProvider Init();
}
