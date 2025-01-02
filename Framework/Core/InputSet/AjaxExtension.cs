namespace Service.Framework.Core.InputSet;

public static class AjaxExtension
{
  public static bool is_ajax_request(this MyInput input)
  {
    try
    {
      var request = input.context.Request;
      return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
    catch
    {
    }

    return false;
  }
}
