using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Service.Framework;

namespace Service.Core.Engine;

public abstract class MyComponentBase : ComponentBase
{
  [Parameter(CaptureUnmatchedValues = true)]
  public IDictionary<string, object>? AdditionalAttributes { get; set; }

  [Inject] public IJSRuntime Js { get; set; }
  [Inject] public NavigationManager NavigationManager { get; set; }

  [Inject] public ILocalStorageService LocalStorage { get; set; }

  // [Inject] public SweetAlertService Swal { get; set; }
  [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; }
  [Inject] public SweetAlertService swal { get; set; }


  public string? Env => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

  private string _imageProfile = "./images/profile.png";


  public List<string> segments
  {
    get
    {
      var currentUrl = NavigationManager.Uri;
      var output = currentUrl.Split('/').Skip(3).ToList();
      return output;
    }
  }

  protected string get(string key)
  {
    var currentUrl = NavigationManager.Uri;
    var queryParams = currentUrl.Split('?')
      .Skip(1)
      .SelectMany(q => q.Split('&'));
    var token = queryParams
      .Select(param => param.Split('='))
      .FirstOrDefault(keyValue => keyValue.Length == 2 && keyValue[0] == key)?[1];
    return string.IsNullOrEmpty(token) ? string.Empty : token;
  }


  public string? get_fragment()
  {
    var uri = NavigationManager.Uri;
    var fragment = NavigationManager.ToAbsoluteUri(uri).Fragment;
    if (string.IsNullOrEmpty(fragment)) return string.Empty;
    if (!fragment.Contains("#")) return string.Empty;
    var parts = fragment.Split('#').ToList();
    return parts.LastOrDefault();
  }

  public void go(string path)
  {
    NavigationManager.NavigateTo(path);
  }

  public List<string> get_query_params()
  {
    var currentUrl = NavigationManager.Uri;
    var queryParams = currentUrl.Split('?')
      .Skip(1)
      .SelectMany(q => q.Split('&'))
      .ToList();
    return queryParams;
  }


  /// <inheritdoc/>
  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();
    // self.navigation = NavigationManager;
  }
}
