namespace Service.Framework.Library.MergeFields;

using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AppMergeFields
{
  private readonly IServiceProvider _serviceProvider;

  // Registered fields from derived classes
  private readonly Dictionary<string, List<MergeField>> _fields = new();

  // Paths to load classes
  private readonly List<string> _registeredPaths = new();

  // Relation for merge fields
  private string _for;

  // All merge fields cached here
  private List<Dictionary<string, List<MergeField>>> _allMergeFields;

  // Helper property for initialization
  private bool _classesForMergeFieldsInitialized = false;

  public AppMergeFields(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;

    // if (HasBuildMethod())
    //   Set(serviceProvider.Build());
    // else
    // Register paths via hooks (extend as needed)
    _registeredPaths = RegisterMergeFields();
  }

  public abstract List<MergeField> Build();

  private bool HasBuildMethod()
  {
    return GetType().GetMethod("Build") != null;
  }

  public List<MergeField> GetByName(string name)
  {
    return _allMergeFields?.SelectMany(feature => feature.TryGetValue(name, out List<MergeField>? fields) ? fields : new List<MergeField>())
      .ToList() ?? new List<MergeField>();
  }

  public Dictionary<string, object> FormatFeature(string name, params object[] parameters)
  {
    if (!_classesForMergeFieldsInitialized)
    {
      all();
      _classesForMergeFieldsInitialized = true;
    }

    var formatted = new Dictionary<string, object>();
    var baseName = GetBaseName(name);
    var mergeFields = GetByName(baseName);
    var uniqueFormatters = mergeFields.Select(f => f.Format.BaseName).Distinct();

    foreach (var formatterName in uniqueFormatters)
    {
      var formatter = GetService(formatterName);

      if (formatter is IFormatter formatterInstance)
      {
        var newFormatted = formatterInstance.Format(parameters);
        if (newFormatted != null)
          foreach (var kvp in newFormatted)
            formatted[kvp.Key] = kvp.Value;
      }
    }

    return formatted;
  }

  public List<MergeField> Get(string name = null)
  {
    var key = string.IsNullOrEmpty(name) ? Name() : name;
    return _fields.TryGetValue(key, out List<MergeField>? fields) ? fields : new List<MergeField>();
  }

  public void Set(List<MergeField> fields)
  {
    var key = Name();
    if (!_fields.ContainsKey(key))
      _fields[key] = fields;
    else
      _fields[key].AddRange(fields);
  }

  public void Register(string loadPath)
  {
    if (!_registeredPaths.Contains(loadPath)) _registeredPaths.Add(loadPath);
  }

  public List<string> GetRegisteredPaths()
  {
    return _registeredPaths;
  }

  public List<Dictionary<string, List<MergeField>>> all(bool rebuild = false)
  {
    if (!rebuild && _allMergeFields != null) return _allMergeFields;

    var available = new List<Dictionary<string, List<MergeField>>>();

    foreach (var path in _registeredPaths)
    {
      var baseName = Load(path);
      var service = GetService(baseName);

      if (service is IMergeFieldProvider provider)
      {
        var fields = provider.GetFields();
        var name = provider.Name();

        var existing = available.FirstOrDefault(a => a.ContainsKey(name));
        if (existing != null)
          existing[name].AddRange(fields);
        else
          available.Add(new Dictionary<string, List<MergeField>> { { name, fields } });
      }
    }

    _allMergeFields = available;
    return _allMergeFields;
  }

  private string Name()
  {
    return _for ??= GetType().Name.Split('_').First().ToLower();
  }

  private string Load(string path)
  {
    var baseName = GetBaseName(path);
    if (GetService(baseName) == null)
      // Load service dynamically or via DI registration
      throw new InvalidOperationException($"Service {baseName} not registered.");

    return baseName;
  }

  private object GetService(string baseName)
  {
    return _serviceProvider.GetService(Type.GetType(baseName));
  }

  private string GetBaseName(string fullName)
  {
    return fullName.Split('.').Last();
  }

  private List<string> RegisterMergeFields()
  {
    // Mocked hook-based registration
    return new List<string> { "merge_fields/client" };
  }

  public interface IFormatter
  {
    Dictionary<string, object> Format(params object[] parameters);
  }

  public interface IMergeFieldProvider
  {
    List<MergeField> GetFields();
    string Name();
  }

  public class MergeField
  {
    public string Name { get; set; }
    public string Key { get; set; }
    public List<string> Available { get; set; }
    public FormatInfo Format { get; set; }
    public bool FromOptions { get; set; }
  }

  public class FormatInfo
  {
    public string BaseName { get; set; }
    public string File { get; set; }
  }
}
