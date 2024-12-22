namespace Service.Components.Pages.Auth;

public class NewPasswordRazorComponent : AppComponentBase
{
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
    await Task.Delay(1000);
    await Js.InvokeVoidAsync("KTAuthNewPassword.init");
  }
}
