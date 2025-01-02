using Service.Entities;
using Service.Framework;

namespace Service.Libraries.MergeField;

public static class AppMergeFieldsExtension
{
  public static AppMergeFields app_merge_fields(this MyInstance self)
  {
    return new AppMergeFields(self);
  }
}

public class AppMergeFields(MyInstance self)
{
  // The actual registered fields from the classes that extend this
  protected Dictionary<string, List<object>> fields = new();

  // Paths to load the classes
  // protected List<string> registered = hooks.apply_filters("register_merge_fields", new List<string>());
  protected List<string> registered
  {
    get
    {
      if (has_build_method())
        set(build());
      return hooks.apply_filters("register_merge_fields", new List<string>());
    }
  }
  // public List<string> getRegistered()
  // {
  //   return registered;
  // }

  // Merge fields relation
  protected string forType = null;

  // All merge fields are stored here
  protected List<object> allMergeFields = null;

  // Helper property
  private bool classesForMergeFieldsInitialized = false;

  public object get_by_name(string name)
  {
    foreach (var feature in all())
      if (feature is Dictionary<string, object> dict && dict.ContainsKey(name))
        return dict[name];
    return null;
  }

  public CustomField format_feature(string name, params object[] parameters)
  {
    if (!classesForMergeFieldsInitialized)
    {
      all();
      classesForMergeFieldsInitialized = true;
    }

    var baseName = file_name(name);
    var mergeFields = get_by_name(baseName);
    // Add logic to process mergeFields
    return new CustomField();
  }

  public List<object> get(string name = null)
  {
    var forType = name ?? get_name();
    return fields.ContainsKey(forType) ? fields[forType] : new List<object>();
  }

  public AppMergeFields set(List<object> newFields)
  {
    var forType = get_name();

    if (!fields.ContainsKey(forType))
      fields[forType] = newFields;
    else
      fields[forType].AddRange(newFields);

    return this;
  }

  public AppMergeFields register(object loadPath)
  {
    if (loadPath is List<string> paths)
      foreach (var path in paths)
        register(path);
    else if (loadPath is string path) registered.Add(path);

    return this;
  }


  public List<object> all(bool reBuild = false)
  {
    if (!reBuild && allMergeFields != null) return allMergeFields;

    var available = new List<object>();

    foreach (var mergeField in registered)
    {
      // Implement load and fetching logic
    }

    // Apply custom fields and return
    return hooks.apply_filters("available_merge_fields", available);
  }

  public string get_name()
  {
    return forType ??= GetType().Name.ToLower().Split('_')[0];
  }

  public string load(string mergeField)
  {
    var baseName = file_name(mergeField);

    // Add logic to dynamically load classes if necessary

    return baseName;
  }

  public List<object> get_flat(List<string> primary, List<string> additional = null, List<string> excludeKeys = null)
  {
    additional ??= new List<string>();
    excludeKeys ??= new List<string>();

    var flat = new List<object>();

    foreach (var registeredItem in all())
    {
      // Implement logic for flattening and filtering fields
    }

    return flat;
  }

  private bool merge_field_exists_by_name(List<object> available, string name)
  {
    foreach (var mergeFields in available)
      if (mergeFields is Dictionary<string, object> dict && dict.ContainsKey(name))
        return true;

    return false;
  }

  private List<object>? check_availability(List<object> fields, string type, List<string> excludeKeys)
  {
    var retVal = new List<object>();

    foreach (var field in fields)
    {
      // Implement availability checks
    }

    return retVal.Count > 0 ? retVal : null;
  }

  private List<object> apply_custom_fields(List<object> registered, object format)
  {
    // Implement custom fields logic
    return registered;
  }

  private bool has_build_method()
  {
    // Check if the build method exists
    return false; // Placeholder implementation
  }

  private List<object> build()
  {
    // Implement build logic if required
    return new List<object>();
  }
}

// Mock hooks class to handle filters
