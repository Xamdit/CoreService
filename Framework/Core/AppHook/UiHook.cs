// <copyright file="UiHook.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace Service.Framework.Core.AppHook;

public class UiHook
{
  private static readonly UiHook InstanceValue = new();
  private readonly ConcurrentDictionary<string, Func<object, Task<bool>>> functionRegistry = new();
  public static UiHook Instance => InstanceValue;

  private UiHook()
  {
  }

  public UiHook Add(string key, Func<object, Task<bool>> func)
  {
    functionRegistry[key] = func;
    return this;
  }

  public async Task<bool> CallAsync(string key, object data = null)
  {
    try
    {
      if (functionRegistry.TryGetValue(key, out var func)) return await func(data);
      return false; // Or throw an exception if the key is not found
    }
    catch
    {
      return false;
    }
  }
}
