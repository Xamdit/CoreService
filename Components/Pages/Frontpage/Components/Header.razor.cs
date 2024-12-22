using Service.Core.Engine;

namespace Service.Components.Pages.Frontpage.Components;

public class HeaderRazor : MyComponentBase
{
  private bool alreadySignedIn = false;

  public bool GetAlreadySignedIn()
  {
    return alreadySignedIn;
  }

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();
    try
    {
      alreadySignedIn = await LocalStorage.ContainKeyAsync("user");
      Console.WriteLine($"alreadySignedIn : {alreadySignedIn}");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }
}
