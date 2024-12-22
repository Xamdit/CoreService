using System.Dynamic;
using Global.Entities;
using Service.Models.Estimates;

namespace Service.Entities;

public class DataSet<T> : DynamicObject
{
  public T? Data { get; set; }
  public List<CustomField> custom_fields { get; set; } = new();
  public List<Taggable> Tags { get; set; } = new();
  public List<Itemable> items { get; set; } = new();
  public List<ItemableOption> NewItems { get; set; } = new();
  public bool SaveAndSend { get; set; } = false;
  public List<Option> Options = new();

  private readonly Dictionary<string, object> _properties = new();
  public List<T> removed_items = new();

  public object this[string key]
  {
    get => _properties[key];
    set
    {
      if (_properties.ContainsKey(key)) _properties.Remove(key);
      _properties.Add(key, value);
    }
  }


  // Override the TryGetMember method to handle property retrieval
  public override bool TryGetMember(GetMemberBinder binder, out object? result)
  {
    return _properties.TryGetValue(binder.Name, out result);
  }

  // Override the TrySetMember method to handle property assignment
  public override bool TrySetMember(SetMemberBinder binder, object? value)
  {
    _properties[binder.Name] = value;
    return true;
  }

  // Override the TryInvokeMember method to handle method calls
  public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
  {
    if (_properties.TryGetValue(binder.Name, out var method) && method is Delegate del)
    {
      result = del.DynamicInvoke(args);
      return true;
    }

    result = null;
    return false;
  }


  public Dictionary<string, object?> GetProperties()
  {
    return new Dictionary<string, object?>(_properties);
  }

  public bool ContainesKey(string key)
  {
    return _properties.ContainsKey(key);
  }
}
