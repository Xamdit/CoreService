using System.Reflection;

namespace Service.Framework.Library.Locales.Providers;

internal class EmbeddedResourceProvider(Assembly hostAssembly, string resourceFolder, IEnumerable<string> knownFileExtensions) : ILocaleProvider
{
  private readonly Dictionary<string, string> _locales = new(); // ie: [es] = "Project.Locales.es.txt"
  private Action<string> _logger;

  public ILocaleProvider SetLogger(Action<string> logger)
  {
    _logger = logger;
    return this;
  }

  public Stream GetLocaleStream(string locale)
  {
    var resourceName = _locales[locale];
    return hostAssembly.GetManifestResourceStream(resourceName);
  }

  public ILocaleProvider Init()
  {
    DiscoverLocales(hostAssembly);
    if (_locales?.Count == 0) throw new I18NException($"{ErrorMessages.NoLocalesFound}: {hostAssembly.FullName}");
    return this;
  }

  public IEnumerable<Tuple<string, string>> GetAvailableLocales()
  {
    return _locales.Select(x =>
    {
      var extension = x.Value.Substring(x.Value.LastIndexOf('.'));
      return new Tuple<string, string>(x.Key, extension);
    });
  }

  private void DiscoverLocales(Assembly hostAssembly)
  {
    _logger?.Invoke("Getting available locales...");
    var localeResources = hostAssembly
      .GetManifestResourceNames()
      .Where(x => x.Contains($".{resourceFolder}."));
    var supportedResources =
      (from name in localeResources
        from extension in knownFileExtensions
        where name.EndsWith(extension)
        select name)
      .ToList();
    if (supportedResources.Count == 0)
      throw new I18NException("No locales have been found. Make sure you've got a folder " +
                              $"called '{resourceFolder}' containing embedded resource files " +
                              $"(with extensions {string.Join(" or ", knownFileExtensions)}) " +
                              "in the host assembly");
    foreach (var resource in supportedResources)
    {
      var parts = resource.Split('.');
      // var localeName = parts[parts.Length - 2];
      var localeName = parts[^2];
      if (_locales.ContainsKey(localeName)) throw new I18NException($"The locales folder '{resourceFolder}' contains a duplicated locale '{localeName}'");
      _locales.Add(localeName, resource);
    }

    _logger?.Invoke($"Found {supportedResources.Count} locales: {string.Join(", ", _locales.Keys.ToArray())}");
  }

  public void Dispose()
  {
    _locales.Clear();
    _logger = null;
  }
}
