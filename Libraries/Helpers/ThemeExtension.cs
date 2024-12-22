namespace Service.Libraries.Helpers;

public static class ThemeExtension
{
  public static void addBodyAttribute(this IJSRuntime js, string attribute, string value)
  {
    js.InvokeVoidAsync("document.body.setAttribute", attribute, value);
  }

  public static void removeBodyAttribute(this IJSRuntime js, string attribute)
  {
    js.InvokeVoidAsync("document.body.classList.removeAttribute", attribute);
  }

  public static void addBodyClass(this IJSRuntime js, string className)
  {
    js.InvokeVoidAsync("document.body.classList.add", className);
  }

  public static void removeBodyClass(this IJSRuntime js, string className)
  {
    js.InvokeVoidAsync("document.body.classList.remove", className);
  }
}
