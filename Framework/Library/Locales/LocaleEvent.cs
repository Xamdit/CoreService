namespace Service.Framework.Library.Locales;

public class LocaleEvents
{
  public event EventHandler<string> LanguageChanged;

  public void InvokeLanguageChanged(string newLanguage, object sender = null)
  {
    Console.WriteLine("Events.InvokeLanguageChanged");
    Console.WriteLine($"{newLanguage}");
    LanguageChanged?.Invoke(sender ?? this, newLanguage);
  }
}
