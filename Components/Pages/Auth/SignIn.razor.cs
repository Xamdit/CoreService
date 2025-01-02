using Service.Core.Extensions;
using Service.Core.Synchronus;


namespace Service.Components.Pages.Auth;

public class SignInComponent : XComponentBase
{
  protected string UserType { get; set; }


  protected string Email { get; set; }


  protected string Password { get; set; }

  protected bool socialSignup = false;
  protected bool canSignup = false;


  protected async Task HandleSubmit()
  {
    Console.WriteLine($"User Type: {UserType}");
    Console.WriteLine($"Email: {Email}");
    Console.WriteLine($"Password: {Password}");
    var auth_model = self.auth_model(db);
    var userSchema = auth_model.Signin(Email, Password, UserType == "admin");
    if (userSchema != null)
    {
      var uuid = userSchema.Uuid;
      // await LocalStorage.SetItemAsync("uuid", userSchema.Uuid);
      // await LocalStorage.SetItemAsync("user_type", UserType);
      try
      {
        var target = userSchema.Type == "admin"
          ? self.navigation.admin_url("dashboard", new { token = uuid })
          : self.navigation.client_url("dashboard", new { token = uuid });
        Console.WriteLine(target);
        self.navigation.NavigateTo(target);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      // await LocalStorage.SetItemAsync("token", response.Token);
      // await LocalStorage.SetItemAsync("user", response.Data);
      // await LocalStorage.SetItemAsStringAsync("type", result.Data.Type);
      // await Swal.FireAsync("Success", "You have successfully signed in", "success");
      // NavigationManager.NavigateTo(url);
    }
  }


  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    UserType = NavigationManager.GetSection();
    // var result = await sync.get_where("users/health", new { }, 10);
    var result = await syncBuilder.get_where("/users/health", new { }, 10);
    Console.WriteLine(result.Content);

    // if (UserType == "admin")
    //   NavigationManager.NavigateTo("/admin");
    if (!firstRender) return;
    // await CheckKeyAndRedirect("user", "/admin/dashboards");
    await Task.Delay(1000);
    // var contain = self.cache.has(ClientControl.ClientType);
    // if (contain)
    // {
    // }
    // await Js.InvokeVoidAsync("KTSigninGeneral.init");
  }
}
