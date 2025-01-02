using Service.Core.Extensions;
using Service.Core.Synchronus;


namespace Service.Components.Pages.Auth;

/// <summary>
/// Represents the sign-in component.
/// </summary>
public class SignInComponent : XComponentBase
{
  /// <summary>
  /// Gets or sets the user type.
  /// </summary>
  protected string UserType { get; set; }


  /// <summary>
  /// Gets or sets the email.
  /// </summary>
  protected string Email { get; set; }

  /// <summary>
  /// Gets or sets the password.
  /// </summary>
  protected string Password { get; set; }

  protected bool socialSignup = false;
  protected bool canSignup = false;

  /// <summary>
  /// Handles the submit action.
  /// </summary>
  /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
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

  /// <summary>
  /// Performs actions after the component has been rendered.
  /// </summary>
  /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
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
