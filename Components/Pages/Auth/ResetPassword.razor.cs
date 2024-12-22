namespace Service.Components.Pages.Auth;

public class ResetPasswordRazorComponent : AppComponentBase
{
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      await Task.Delay(1000);
      await Js.InvokeVoidAsync("KTAuthResetPassword.init");
    }
  }
}
