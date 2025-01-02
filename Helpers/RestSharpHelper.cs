using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class RestSharpHelper
{
  public static RestClient rest_client(this HelperBase helper, string url)
  {
    var options = new RestClientOptions(url)
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    return client;
  }

  public static RestClient rest_client_google(this HelperBase helper)
  {
    var options = new RestClientOptions("https://www.google.com")
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    return client;
  }

  public static RestClient rest_client(this HelperBase helper)
  {
    var options = new RestClientOptions("https://localhost:5000")
    {
      MaxTimeout = -1
    };
    var client = new RestClient(options);
    return client;
  }

  public static async Task<(bool is_success, T data)> rest_client_json<T>(this HelperBase helper, string route, Method method, object data = null) where T : class
  {
    var client = helper.rest_client("https://localhost:5000");
    var request = new RestRequest(route, method);
    request.AddHeader("Content-Type", "application/json");
    if (data != null)
    {
      var body = JsonConvert.SerializeObject(data);
      request.AddStringBody(body, DataFormat.Json);
    }

    var response = await client.ExecuteAsync(request);
    return (response.StatusCode == HttpStatusCode.OK, JsonConvert.DeserializeObject<T>(response.Content));
  }
}
