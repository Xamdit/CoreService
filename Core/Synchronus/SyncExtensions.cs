using System.ComponentModel;
using System.Dynamic;
using Newtonsoft.Json;
using RestSharp;

namespace Service.Core.Synchronus;

public static class SyncExtensions
{
  public static async Task<SyncResult> post(this SyncBuilder self, string service, dynamic data = default(ExpandoObject))
  {
    var options = new RestClientOptions(self.gateway)
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    var request = new RestRequest(service, Method.Post);
    request.AddHeader("accept", "*/*");
    request.AddHeader("Content-Type", "application/json");
    var body = JsonConvert.SerializeObject(data);
    RestRequestExtensions.AddStringBody(request, body, DataFormat.Json);
    var response = await client.ExecuteAsync(request);
    var result = new SyncResult(response, self);
    Console.WriteLine(result.Content);
    return result;
  }

  public static SyncBuilder get(this SyncBuilder self, string service, int limit = 0, object offset = null)
  {
    return self;
  }

  public static async Task<SyncResult> get_where(this SyncBuilder self, string service, dynamic condition = default(ExpandoObject), int limit = 0, object offset = null)
  {
    var options = new RestClientOptions(self.gateway)
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    var path = service;
    var args = "";
    foreach (var desc in TypeDescriptor.GetProperties(condition))
    {
      var key = desc.Name;
      var value = desc.GetValue(condition);
      args += $"{key}={value}&";
    }

    if (!string.IsNullOrEmpty(args)) path += "?" + args;
    var request = new RestRequest(path);
    request.AddHeader("accept", "*/*");
    var response = await client.ExecuteAsync(request);
    var result = new SyncResult(response, self);
    Console.WriteLine(result.Content);
    return result;
  }


  public static async Task<SyncResult> put(this SyncBuilder self, string service, dynamic data = default(ExpandoObject), dynamic condition = default(ExpandoObject))
  {
    var options = new RestClientOptions(self.gateway)
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    var request = new RestRequest(service, Method.Put);
    request.AddHeader("accept", "*/*");
    request.AddHeader("Content-Type", "application/json");
    var dataset = new { data, condition };
    var body = JsonConvert.SerializeObject(dataset);
    request.AddStringBody(body, DataFormat.Json);
    var response = await client.ExecuteAsync(request);
    var result = new SyncResult(response, self);
    Console.WriteLine(result.Content);
    return result;
  }

  public static async Task<SyncResult> delete_where(this SyncBuilder self, string service, dynamic condition = default(ExpandoObject), int limit = 0, object offset = null)
  {
    var options = new RestClientOptions(self.gateway)
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    var path = service;
    var args = "";
    foreach (var desc in TypeDescriptor.GetProperties(condition))
    {
      var key = desc.Name;
      var value = desc.GetValue(condition);
      args += $"{key}={value}&";
    }

    if (!string.IsNullOrEmpty(args)) path += "?" + args;
    var request = new RestRequest(path, Method.Delete);
    request.AddHeader("accept", "*/*");
    var response = await client.ExecuteAsync(request);
    var result = new SyncResult(response, self);
    Console.WriteLine(result.Content);
    return result;
  }

  public static int count_all_results(this SyncBuilder self, string table = "", bool reset = true)
  {
    return 0;
  }


  public static void display_error(this SyncBuilder self, string message)
  {
    Console.WriteLine($"error: {message}");
  }

  public static void show_error(this SyncBuilder self, string message)
  {
    Console.WriteLine($"error: {message}");
  }
}
