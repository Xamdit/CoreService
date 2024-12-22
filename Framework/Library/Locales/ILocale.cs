using System.Reflection;

namespace Service.Framework.Library.Locales;

public interface ILocale
{
  string this[string key] { get; }
  List<PortableLanguage> Languages { get; }
  PortableLanguage Language { get; set; }
  ILocale SetInstance(MyInstance instance);
  ILocale SetNotFoundSymbol(string symbol);
  ILocale SetLogger(Action<string> output);
  ILocale SetThrowWhenKeyNotFound(bool enabled);
  ILocale SetFallbackLocale(string locale);
  ILocale SetResource(string source);
  ILocale Build();
  string GetDefaultLocale();
  string Translate(string key, params object[] args);
  string TranslateOrNull(string key, params object[] args);
  Dictionary<TEnum, string> TranslateEnumToDictionary<TEnum>();
  List<string> TranslateEnumToList<TEnum>();
  List<Tuple<TEnum, string>> TranslateEnumToTupleList<TEnum>();
  ILocale Init(Assembly hostAssembly);
}
