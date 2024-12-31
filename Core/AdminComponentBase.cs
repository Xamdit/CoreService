using Microsoft.AspNetCore.Components;
using Service.Components.Layout;
using Service.Entities;
using Task = System.Threading.Tasks.Task;


namespace Service.Core;

public abstract class AdminComponentBase : XComponentBase
{
  [CascadingParameter] public DefaultDarkSidebar CurrentLayout { get; set; }
  public Staff user { get; set; }

  private bool IsValidGuid
  {
    get
    {
      if (string.IsNullOrEmpty(token)) return false;
      user = db.Staff.First(x => x.Uuid == token);
      return true;
    }
  }


  protected void GoToSignin()
  {
    NavigationManager.NavigateTo("/client");
  }

  protected void Logout()
  {
    NavigationManager.NavigateTo("/signout");
  }

  /// <inheritdoc/>
  protected override async void OnInitialized()
  {
    await Task.Delay(3000);
    try
    {
      await Js.InvokeVoidAsync("document.body.removeAttribute", "data-kt-app-reset-transition");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }

    try
    {
      await Js.InvokeVoidAsync("document.body.removeAttribute", "data-kt-app-page-loading");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }

  /// <inheritdoc/>
  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();
    try
    {
      user = db.Staff.First(x => x.Uuid == token);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }

  /// <inheritdoc/>
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender) StateHasChanged();
  }
}
