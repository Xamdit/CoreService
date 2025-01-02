using Newtonsoft.Json;
using Service.Framework.Library.Merger;

namespace Service.Framework.Core.AppHook;

public class HookItem
{
  public string Name { get; set; }
  public string Items { get; set; }
}

public class Hooks()
{
  public bool Enabled { get; private set; } = false;
  private readonly Dictionary<string, SortedDictionary<int, List<Filter>>> filters = new();
  private readonly Dictionary<string, bool> mergedFilters = new();
  private readonly Dictionary<string, int> actions = new();
  private readonly HashSet<string> currentFilter = new();
  public Dictionary<string, object> HooksDictionary { get; private set; } = new();
  protected Dictionary<string, object> _objects = new();
  protected bool _inProgress = false;
  private List<HookItem> items { get; set; } = new();

  // public Hooks()
  // {
  //   log("Hooks Class Initialized");
  //   // Check if hooks are enabled in the configuration
  //   if (self.config["enable_hooks"].Is(false)) return;
  //   LoadHooksConfiguration();
  //   if (HooksDictionary.Count == 0) return;
  //   Enabled = true;
  // }

  protected void LoadHooksConfiguration()
  {
    // Load hooks from configuration files
    var hooksPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs/hooks.cs");
    if (File.Exists(hooksPath)) LoadHooks(hooksPath);

    var envHooksPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"configs/{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}/hooks.cs");
    if (File.Exists(envHooksPath)) LoadHooks(envHooksPath);
  }

  protected void LoadHooks(string path)
  {
    // Logic to load hooks from the specified path
    // This may involve reading a file and parsing it into HooksDictionary
  }

  public bool CallHook(string which)
  {
    if (!Enabled || !HooksDictionary.ContainsKey(which)) return false;

    if (HooksDictionary[which] is List<object> hookList && hookList.Count > 0)
      foreach (var val in hookList)
        RunHook(val);
    else
      RunHook(HooksDictionary[which]);

    return true;
  }

  protected bool RunHook(object data)
  {
    if (data is Delegate del)
    {
      del.DynamicInvoke();
      return true;
    }

    if (data is not Dictionary<string, object> hookData) return false;

    // Safety - Prevents run-away loops
    if (_inProgress) return false;

    // Ensure the hook has the required parameters
    if (!hookData.TryGetValue("filepath", out var filepath) || !hookData.TryGetValue("filename", out var filename)) return false;

    var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (string)filepath, (string)filename);

    if (!File.Exists(fullPath)) return false;

    // Retrieve class, function, and parameters
    var className = hookData.ContainsKey("class") ? (string)hookData["class"] : null;
    var functionName = hookData.ContainsKey("function") ? (string)hookData["function"] : null;
    var parameters = hookData.ContainsKey("params") ? hookData["params"] : null;

    if (string.IsNullOrEmpty(functionName)) return false;

    _inProgress = true;

    if (!string.IsNullOrEmpty(className))
    {
      if (_objects.ContainsKey(className))
      {
        var instance = _objects[className];
        var method = instance.GetType().GetMethod(functionName);
        if (method != null)
        {
          method.Invoke(instance, new[] { parameters });
        }
        else
        {
          _inProgress = false;
          return false;
        }
      }
      else
      {
        // Load class if not already loaded
        var classType = Type.GetType(className) ?? LoadClass(fullPath);
        if (classType == null || !classType.GetMethod(functionName).IsPublic)
        {
          _inProgress = false;
          return false;
        }

        var instance = Activator.CreateInstance(classType);
        _objects[className] = instance;
        classType.GetMethod(functionName).Invoke(instance, new[] { parameters });
      }
    }
    else
    {
      // Handle global functions
      var globalFunction = GetGlobalFunction(functionName, fullPath);
      if (globalFunction == null)
      {
        _inProgress = false;
        return false;
      }

      globalFunction.DynamicInvoke(parameters);
    }

    _inProgress = false;
    return true;
  }

  private Delegate GetGlobalFunction(string functionName, string fullPath)
  {
    // Load global function logic here (like including a file in PHP)
    return null; // Return the function delegate if it exists
  }

  private Type LoadClass(string fullPath)
  {
    // Implement logic to dynamically load class from the file path
    return null; // Return the loaded class type
  }

  private void log(string message)
  {
    // Implement logging mechanism
    Console.WriteLine(message);
  }

  public T apply_filters<T>(string filter_name, params object[] items) where T : class, new()
  {
    var output = new object();
    items.ToList().ForEach(x => { output = TypeMerger.Merge(output, x); });
    var h = new HookItem();
    h.Name = filter_name;
    h.Items = JsonConvert.SerializeObject(output);
    this.items.Add(h);
    return (T)Convert.ChangeType(output, typeof(T));
  }


  public bool AddFilter(string tag, Func<object, object> functionToAdd, int priority = 10, int acceptedArgs = 1)
  {
    if (string.IsNullOrEmpty(tag) || functionToAdd == null) return false;

    if (!filters.ContainsKey(tag))
      filters[tag] = new SortedDictionary<int, List<Filter>>();
    if (!filters[tag].ContainsKey(priority))
      filters[tag][priority] = new List<Filter>();

    filters[tag][priority].Add(new Filter(functionToAdd, acceptedArgs));
    mergedFilters.Remove(tag);
    return true;
  }

  public bool RemoveFilter(string tag, Func<object, object> functionToRemove, int priority = 10)
  {
    if (string.IsNullOrEmpty(tag) || functionToRemove == null) return false;
    if (!filters.ContainsKey(tag) || !filters[tag].ContainsKey(priority)) return false;
    var filterToRemove = filters[tag][priority].FirstOrDefault(f => f.Function == functionToRemove);
    if (filterToRemove == null) return false;
    filters[tag][priority].Remove(filterToRemove);
    if (filters[tag][priority].Count == 0)
      filters[tag].Remove(priority);

    mergedFilters.Remove(tag);
    return true;
  }

  public bool RemoveAllFilters(string tag, int? priority = null)
  {
    if (string.IsNullOrEmpty(tag)) return false;

    if (!filters.ContainsKey(tag)) return true;

    if (priority.HasValue)
    {
      if (filters[tag].ContainsKey(priority.Value))
        filters[tag].Remove(priority.Value);
    }
    else
    {
      filters.Remove(tag);
    }

    mergedFilters.Remove(tag);
    return true;
  }

  public bool HasFilter(string tag, Func<object, object>? functionToCheck = null)
  {
    if (string.IsNullOrEmpty(tag)) return false;

    if (!filters.ContainsKey(tag)) return false;

    return functionToCheck == null || filters[tag].Values.Any(filtersList =>
      filtersList.Any(f => f.Function == functionToCheck));
  }

  public dynamic apply_filters(string tag, params object[] values)
  {
    if (string.IsNullOrEmpty(tag))
      return new DynamicWrapper(values);
    var args = new object[] { values };

    if (filters.ContainsKey("all"))
    {
      currentFilter.Add(tag);
      CallAllHooks(args);
    }

    if (!filters.ContainsKey(tag))
    {
      if (filters.ContainsKey("all")) currentFilter.Remove(tag);
      return new DynamicWrapper(values);
    }

    if (!mergedFilters.ContainsKey(tag))
    {
      filters[tag] = new SortedDictionary<int, List<Filter>>(filters[tag]);
      mergedFilters[tag] = true;
    }

    ExecuteFilters(tag, args);
    currentFilter.Remove(tag);
    var output = new DynamicWrapper(values);
    return output.ToDynamic();
  }

  public bool AddAction(string tag, Action functionToAdd, int priority = 10)
  {
    return AddFilter(tag, value =>
    {
      functionToAdd();
      return value;
    }, priority);
  }

  public bool HasAction(string tag, Action? functionToCheck = null)
  {
    return HasFilter(tag, value =>
    {
      functionToCheck?.Invoke();
      return value;
    });
  }

  public bool RemoveAction(string tag, Action functionToRemove, int priority = 10)
  {
    return RemoveFilter(tag, value =>
    {
      functionToRemove?.Invoke();
      return value;
    }, priority);
  }

  public void do_action(string tag, params object[] args)
  {
    if (!actions.ContainsKey(tag))
      actions[tag] = 1;
    else
      actions[tag]++;

    if (filters.ContainsKey("all"))
    {
      currentFilter.Add(tag);
      CallAllHooks(args);
    }

    if (!filters.ContainsKey(tag))
    {
      if (filters.ContainsKey("all")) currentFilter.Remove(tag);
      return;
    }

    ExecuteFilters(tag, args);

    currentFilter.Remove(tag);
  }

  public int DidAction(string tag)
  {
    return actions.TryGetValue(tag, out var count) ? count : 0;
  }

  public string? CurrentFilter()
  {
    return currentFilter.LastOrDefault();
  }

  private void ExecuteFilters(string tag, object[] args)
  {
    filters[tag]
      .SelectMany(priority => filters[tag][priority.Key]
        .Where(filter => filter.Function != null))
      .ToList()
      .ForEach(filter => args[0] = filter.Function(args[0]));
  }

  private string BuildUniqueId(string tag, Func<object, object> function, object priority)
  {
    return function.GetHashCode().ToString();
  }

  private void CallAllHooks(object[] args)
  {
    if (!filters.ContainsKey("all")) return;

    filters["all"]
      .SelectMany(priority => filters["all"][priority.Key]
        .Where(filter => filter.Function != null))
      .ToList()
      .ForEach(filter => filter.Function(args[0]));
  }

  private class Filter
  {
    public Func<object, object> Function { get; }
    public int AcceptedArgs { get; }

    public Filter(Func<object, object> function, int acceptedArgs)
    {
      Function = function ?? throw new ArgumentNullException(nameof(function));
      AcceptedArgs = acceptedArgs;
    }
  }
}
