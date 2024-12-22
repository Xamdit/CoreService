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

  public static RestClient rest_client_goole(this HelperBase helper)
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
}
