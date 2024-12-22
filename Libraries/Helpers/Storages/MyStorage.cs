using System.Text.Json;

namespace Service.Libraries.Helpers.Storages;

public static class StorageService
{
  // Save data to localStorage
  public static async Task SetLocalAsync<T>(this IJSRuntime _jsRuntime, string key, T value)
  {
    var jsonData = JsonSerializer.Serialize(value);
    await _jsRuntime.InvokeVoidAsync("StorageUtil.setLocal", key, jsonData);
  }

  // Get data from localStorage
  public static async Task<T?> GetLocalAsync<T>(this IJSRuntime _jsRuntime, string key)
  {
    var jsonData = await _jsRuntime.InvokeAsync<string>("StorageUtil.getLocal", key);
    return jsonData != null ? JsonSerializer.Deserialize<T>(jsonData) : default;
  }

  // Remove data from localStorage
  public static async Task RemoveLocalAsync(this IJSRuntime _jsRuntime, string key)
  {
    await _jsRuntime.InvokeVoidAsync("StorageUtil.removeLocal", key);
  }

  // Clear all data in localStorage
  public static async Task ClearLocalAsync(this IJSRuntime _jsRuntime)
  {
    await _jsRuntime.InvokeVoidAsync("StorageUtil.clearLocal");
  }

  // Save data to sessionStorage
  public static async Task SetSessionAsync<T>(this IJSRuntime _jsRuntime, string key, T value)
  {
    var jsonData = JsonSerializer.Serialize(value);
    await _jsRuntime.InvokeVoidAsync("StorageUtil.setSession", key, jsonData);
  }

  // Get data from sessionStorage
  public static async Task<T?> GetSessionAsync<T>(this IJSRuntime _jsRuntime, string key)
  {
    var jsonData = await _jsRuntime.InvokeAsync<string>("StorageUtil.getSession", key);
    return jsonData != null ? JsonSerializer.Deserialize<T>(jsonData) : default;
  }

  // Remove data from sessionStorage
  public static async Task RemoveSessionAsync(this IJSRuntime _jsRuntime, string key)
  {
    await _jsRuntime.InvokeVoidAsync("StorageUtil.removeSession", key);
  }

  // Clear all data in sessionStorage
  public static async Task ClearSessionAsync(this IJSRuntime _jsRuntime)
  {
    await _jsRuntime.InvokeVoidAsync("StorageUtil.clearSession");
  }
}
