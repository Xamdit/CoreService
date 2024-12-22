using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Service.Components.Layout;

public class FrontendLayoutRazor : LayoutComponentBase
{
  [Inject] private IJSRuntime Js { get; set; }
  [Inject] private ILocalStorageService localStorage { get; set; }
  [Inject] private NavigationManager navigationManager { get; set; }

  protected override void OnAfterRender(bool firstRender)
  {
    Js.InvokeVoidAsync(
      "document.body.classList.add",
      "app-black", "bgi-size-cover",
      "bgi-position-center",
      "bgi-no-repeat"
    );
  }


  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
    await Task.Delay(300);
    await Js.InvokeVoidAsync("document.body.removeAttribute", "data-kt-app-reset-transition");
    await Js.InvokeVoidAsync("document.body.removeAttribute", "data-kt-app-page-loading");
  }
}
