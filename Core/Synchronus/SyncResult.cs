using System.Net;
using RestSharp;

namespace Service.Core.Synchronus;

public class SyncResult(RestResponse response, SyncBuilder builder)
{
  public HttpStatusCode StatusCode { get; } = response.StatusCode;
  public string? Content => response?.StatusCode == HttpStatusCode.OK ? response?.Content : "";
  public string? ErrorMessage => response.StatusCode != HttpStatusCode.OK ? response.ErrorMessage : null;
  public bool Success => response?.StatusCode == HttpStatusCode.OK;
}
