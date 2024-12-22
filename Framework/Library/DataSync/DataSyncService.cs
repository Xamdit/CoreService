using Newtonsoft.Json;
using RestSharp;

namespace Service.Framework.Library.DataSync;

public class DataSyncService(string apiUrl, ILogger logger)
{
  private readonly RestClient client = new(apiUrl);

  private static RestRequest BuildRequest(string url, Method method, object? data = null)
  {
    var request = new RestRequest(url, method);
    if (data != null && method is Method.Post or Method.Put) request.AddJsonBody(data);
    return request;
  }

  private async Task<T?> SendRequestAsync<T>(string url, Method method, object? data = null)
  {
    var request = BuildRequest(url, method, data);
    try
    {
      var response = client.Execute(request);
      if (response.IsSuccessful)
      {
        Console.WriteLine("success response from server");
        if (typeof(T) == typeof(string)) return (T?)(object?)response.Content;
        var content = $"{response.Content}";
        Console.WriteLine(content);
        var jsonObj = JsonConvert.DeserializeObject<T>(content);
        return jsonObj;
      }

      logger.LogError("Request to {Url} failed with status code {StatusCode}: {Content}", url, response.StatusCode, response.Content);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while making request to {Url}", url);
    }

    return default;
  }

  public Task<T?> Get<T>(string url, object? data = null)
  {
    return SendRequestAsync<T>(url, Method.Get, data);
  }

  public Task<T?> Post<T>(string url, object? data = null)
  {
    return SendRequestAsync<T>(url, Method.Post, data);
  }

  public Task<T?> Put<T>(string url, object data)
  {
    return SendRequestAsync<T>(url, Method.Put, data);
  }

  public Task<T?> Delete<T>(string url)
  {
    return SendRequestAsync<T>(url, Method.Delete);
  }

  public async Task<string> Get(string url, object? data = null)
  {
    return await SendRequestAsync<string>(url, Method.Get, data) ?? string.Empty;
  }

  public async Task<string> Post(string url, object? data = null)
  {
    return await SendRequestAsync<string>(url, Method.Post, data) ?? string.Empty;
  }

  public async Task<string> Put(string url, object data)
  {
    return await SendRequestAsync<string>(url, Method.Put, data) ?? string.Empty;
  }

  public async Task<string> Delete(string url)
  {
    return await SendRequestAsync<string>(url, Method.Delete) ?? string.Empty;
  }
}
