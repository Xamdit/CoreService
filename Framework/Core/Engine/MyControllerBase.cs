using System.Dynamic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Service.Framework.Schemas;

namespace Service.Framework.Core.Engine;

public static class ResponseType
{
  public static string Error = "error";
  public static string Json = "json";
  public static string Display = "display";
  public static string Download = "download";
}

public abstract class MyControllerBase : ControllerBase
{
  protected dynamic data = new ExpandoObject();
  protected string token = string.Empty;
  protected string HTTP_REFERER => HttpContext.Request.Headers["Referer"].ToString();

  protected string BearerToken =>
    HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);


  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  public abstract void Init();

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected HttpResponseMessage MakeHtml(string htmlContent)
  {
    var response = new HttpResponseMessage(HttpStatusCode.OK);
    response.Content = new StringContent(htmlContent, Encoding.UTF8, "text/html");
    return response;
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult PermissionDeny()
  {
    return MakeError("Permission Deny");
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult MakeBoolean(bool response = false)
  {
    var json = new { Success = true, Type = ResponseType.Json, token, Data = response };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult MakeSuccess<T>(T response, bool rawdata = true)
    where T : class?
  {
    if (rawdata)
      return new JsonResult(response) { ContentType = "application/json" };
    var json = new SuccessResponse<T>
    {
      Success = true,
      Type = ResponseType.Json,
      Token = token,
      Data = response
    };

    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult MakeResult(object response)
  {
    // response = set_value(response, "token", jwt);
    return new JsonResult(response) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult MakeError(string message = "")
  {
    Response.StatusCode = 404;
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = message
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult MakeError(string message, HttpStatusCode code)
  {
    Response.StatusCode = (int)code;
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = message
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult MakeDeny(object? response = null)
  {
    var json = new
    {
      Success = true,
      Type = ResponseType.Json,
      data = response
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult Show_404(string message = "")
  {
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = message
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected JsonResult set_alert(string message = "")
  {
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = message
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected void set_alert(string type, string message)
  {
    // try
    // {
    //   if (data.alert == null) data.alert = new List<string>();
    // }
    // catch
    // {
    //   data.alert = new List<string>();
    // }
  }

  protected void set_alert(bool type, string message)
  {
    // try
    // {
    //   if (data.alert == null) data.alert = new List<string>();
    // }
    // catch
    // {
    //   data.alert = new List<string>();
    // }
  }

  /**
 * Error Handler
 *
 * This function lets us invoke the exception class and
 * display errors using the standard error template located
 * in application/views/errors/error_general.php
 * This function will send the error page directly to the
 * browser and exit.
 *
 * @param   string
 * @param   int
 * @param   string
 * @return  void
 */
  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected void show_error(string message, int statusCode = 500, string heading = "An Error Was Encountered")
  {
    statusCode = Math.Abs(statusCode);
    if (statusCode >= 100) return;
    var exitStatus = statusCode + 9; // 9 is EXIT__AUTO_MIN
    statusCode = 500;
    // echo $_error->show_error($heading, $message, 'error_general', $status_code);
    // exit($exit_status);
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected IActionResult access_denied(string pageName = "")
  {
    Response.StatusCode = 400;
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = $"Access Denied For {pageName}"
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected IActionResult Blank_page(string pageName, string? info = null)
  {
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = $"Access Denied For {pageName}"
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  [ApiExplorerSettings(IgnoreApi = true)] // Add this attribute to ignore the method from Swagger
  protected IActionResult Die(string message = "")
  {
    Response.StatusCode = 404;
    var json = new
    {
      Success = false,
      Type = ResponseType.Json,
      Message = message
    };
    return new JsonResult(json) { ContentType = "application/json" };
  }

  public MyControllerBase(ILogger<MyControllerBase> logger, MyInstance self )
  {
    Init();
  }
}
