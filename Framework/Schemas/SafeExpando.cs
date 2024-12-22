// <copyright file="SafeExpando.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Dynamic;

namespace Service.Framework.Schemas;

public class SafeExpando : DynamicObject
{
  private readonly ExpandoObject expando = new();

  public override bool TryGetMember(GetMemberBinder binder, out object result)
  {
    // Try to get the value from the ExpandoObject
    var expandoDict = expando as IDictionary<string, object>;
    if (expandoDict.TryGetValue(binder.Name, out result)) return true;

    // Return null if the key does not exist
    result = null;
    return true;
  }

  public override bool TrySetMember(SetMemberBinder binder, object value)
  {
    var expandoDict = expando as IDictionary<string, object>;
    expandoDict[binder.Name] = value;
    return true;
  }

  // If you need to expose the underlying ExpandoObject or its members
  public ExpandoObject UnderlyingExpandoObject => expando;
}
