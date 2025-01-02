using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Service.Framework.Core.Entities;
using Service.Framework.Core.Extensions;

namespace Service.Framework.Library.Locales;

public class Locale : ILocale
{
  private Dictionary<string, string> dataset = new();
  private string currentLocale = "en";
  private MyInstance self { get; set; }
  public static ILocale Current { get; set; } = new Locale();
  public event PropertyChangedEventHandler PropertyChanged;
  public List<Text> texts = new();


  private void NotifyPropertyChanged(string info)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
  }

  public string this[string key]
    => Translate(key);

  public PortableLanguage Language
  {
    get => Languages?.FirstOrDefault(x => x.Locale.Equals(locale));
    set
    {
      if (Language.Locale == value.Locale)
      {
        Log($"{value.DisplayName} is the current language. No actions will be taken");
        return;
      }

      LoadLocale(value.Locale);
      NotifyPropertyChanged(nameof(Locale));
      NotifyPropertyChanged(nameof(Language));
    }
  }

  private string _locale;

  public string locale
  {
    get => _locale;
    set
    {
      if (_locale == value)
      {
        Log($"{value} is the current locale. No actions will be taken");
        return;
      }

      LoadLocale(value);
      NotifyPropertyChanged(nameof(Locale));
      NotifyPropertyChanged(nameof(Language));
    }
  }


  public List<PortableLanguage> Languages
  {
    get
    {
      var output = _locales
        .Select(x => new PortableLanguage
        {
          Locale = x,
          DisplayName = TranslateOrNull(x)
        })
        .ToList();
      return output;
    }
  }

  private List<Text> _translations = new();
  private readonly IList<ILocaleProvider> _providers = new List<ILocaleProvider>();
  private readonly IList<Tuple<ILocaleReader, string>> _readers = new List<Tuple<ILocaleReader, string>>();
  private List<string?> _locales = new();
  private Dictionary<string, string> _localeFileExtensionMap;
  private string _notFoundSymbol = "?";
  private string _fallbackLocale;
  private Action<string> _logger;

  #region Fluent API

  public ILocale SetInstance(MyInstance instance)
  {
    self = instance;
    return this;
  }

  public ILocale SetNotFoundSymbol(string symbol)
  {
    if (!string.IsNullOrEmpty(symbol))
      _notFoundSymbol = symbol;
    return this;
  }

  public ILocale SetLogger(Action<string> output)
  {
    _logger = output;
    return this;
  }

  public ILocale SetThrowWhenKeyNotFound(bool enabled)
  {
    return this;
  }

  public ILocale SetFallbackLocale(string locale)
  {
    _fallbackLocale = locale;
    this.locale = locale;
    return this;
  }

  public ILocale SetResource(string source)
  {
    // var jsonObject = client.CallAsync(source).Result;
    // Console.WriteLine(jsonObject.label); // Should output "Hello World"
    return this;
  }


  public ILocale Build()
  {
    Console.WriteLine("Database path configured and context created successfully.");
    return this;
  }

  public ILocale AddLocaleReader(ILocaleReader reader, string extension)
  {
    if (reader == null)
      throw new I18NException(ErrorMessages.ReaderNull);
    if (string.IsNullOrEmpty(extension))
      throw new I18NException(ErrorMessages.ReaderExtensionNeeded);
    if (!extension.StartsWith("."))
      throw new I18NException(ErrorMessages.ReaderExtensionStartWithDot);
    if (extension.Length < 2)
      throw new I18NException(ErrorMessages.ReaderExtensionOneChar);
    if (extension.Split('.').Length - 1 > 1)
      throw new I18NException(ErrorMessages.ReaderExtensionJustOneDot);
    if (_readers.Any(x => x.Item2.Equals(extension)))
      throw new I18NException(ErrorMessages.ReaderExtensionTwice);
    if (_readers.Any(x => x.Item1 == reader))
      throw new I18NException(ErrorMessages.ReaderTwice);
    _readers.Add(new Tuple<ILocaleReader, string>(reader, extension));
    return this;
  }


  public ILocale Init()
  {
    var localeToLoad = GetDefaultLocale();
    if (string.IsNullOrEmpty(localeToLoad))
    {
      if (!string.IsNullOrEmpty(_fallbackLocale) && _locales.Contains(_fallbackLocale))
      {
        localeToLoad = _fallbackLocale;
        Log($"Loading fallback locale: {_fallbackLocale}");
      }
      else
      {
        localeToLoad = _locales.ElementAt(0);
        Log($"Loading first locale on the list: {localeToLoad}");
      }
    }
    else
    {
      Log($"Default locale from current culture: {localeToLoad}");
    }

    LoadLocale(localeToLoad);
    NotifyPropertyChanged(nameof(Locale));
    NotifyPropertyChanged(nameof(Language));
    return this;
  }

  #endregion

  #region Load stuff

  private void LoadLocale(string locale)
  {
    currentLocale = locale;
    try
    {
      // _locales = self.fw.Texts.GroupBy(x => x.Locale).Select(x => x.First()).Select(x => x.Locale).ToList();
      _locales = self.fw.Texts.Select(x => x.Locale).ToList().Distinct().ToList();
      // if (!_locales.Contains(locale))
      //   throw new I18NException($"Locale '{locale}' is not available", new KeyNotFoundException());
      try
      {
        dataset.TryGetValue(locale, out var value);
        // _translations = rows.Where(x => x.Locale == locale).ToList();
      }
      catch (Exception e)
      {
        // var message = $"{ErrorMessages.ReaderException}.\nReader: {reader.GetType().Name}.\nLocale: {locale}{extension}";
        var message = "";
        throw new I18NException(message, e);
      }

      LogTranslations();
      _locale = locale;
      NotifyPropertyChanged("Item[]");
    }
    catch (Exception ex)
    {
    }
  }

  #endregion


  public string Translate(string key, params object[] args)
  {
    if (string.IsNullOrEmpty(key)) return "";

    var row = self.fw.Texts.FirstOrDefault(x => x.Locale == _locale && x.Key == key);
    if (row == null)
    {
      ignore(() =>
      {
        var sender = new Text
        {
          Locale = _locale,
          Key = key,
          Value = key
        };
        self.fw.Texts.Add(sender);
        self.fw.SaveChanges();
      });

      return key;
    }

    var output = $"{row.Value}";
    if (args.ToList().Any()) output = self.parse($"{row.Value}", args);

    return output;
  }


  public string TranslateOrNull(string key, params object[] args)
  {
    return _translations.Any(x => x.Locale == currentLocale && x.Key == key)
      ? args.Length == 0
        ? _translations.FirstOrDefault(x => x.Locale == currentLocale && x.Key == key)?.Value
        : string.Format(_translations.FirstOrDefault(x => x.Locale == currentLocale && x.Key == key)?.Value, args)
      : null;
  }

  public Dictionary<TEnum, string> TranslateEnumToDictionary<TEnum>()
  {
    var type = typeof(TEnum);
    var dic = new Dictionary<TEnum, string>();
    foreach (var value in Enum.GetValues(type))
    {
      var name = Enum.GetName(type, value);
      dic.Add((TEnum)value, Translate($"{type.Name}.{name}"));
    }

    return dic;
  }

  public List<string> TranslateEnumToList<TEnum>()
  {
    var type = typeof(TEnum);
    return Enum.GetValues(type)
      .Cast<object>()
      .Select(value => Enum.GetName(type, value))
      .Select(name => Translate($"{type.Name}.{name}"))
      .ToList();
  }


  public List<Tuple<TEnum, string>> TranslateEnumToTupleList<TEnum>()
  {
    var type = typeof(TEnum);
    return Enum.GetValues(type)
      .Cast<TEnum>()
      .Select(value => Tuple.Create(value, Translate($"{type.Name}.{Enum.GetName(type, value)}")))
      .ToList();
  }

  public ILocale Init(Assembly hostAssembly)
  {
    return this;
  }

  #region Helpers

  public string GetDefaultLocale()
  {
    var currentCulture = CultureInfo.CurrentCulture;
    return _locales.FirstOrDefault(x => x.Equals(currentCulture.Name)) ?? _locales.FirstOrDefault(x => x.Equals(currentCulture.TwoLetterISOLanguageName));
  }

  #endregion

  #region Logging

  private void LogTranslations()
  {
    Log("========== I18NPortable translations ==========");
    foreach (var item in _translations)
      Log($"{item.Key} = {item.Value}");
    Log("====== I18NPortable end of translations =======");
  }

  private void Log(string trace)
  {
    _logger?.Invoke($"[{nameof(Locale)}] {trace}");
  }

  #endregion

  #region Cleanup

  public void Dispose()
  {
    if (PropertyChanged != null)
    {
      foreach (var @delegate in PropertyChanged.GetInvocationList()) PropertyChanged -= (PropertyChangedEventHandler)@delegate;
      PropertyChanged = null;
    }

    _translations?.Clear();
    _locales?.Clear();
    _readers?.Clear();
    _localeFileExtensionMap?.Clear();
    _locale = null;
    Current = null;
    Log("Disposed");
    _logger = null;
  }

  #endregion
}
