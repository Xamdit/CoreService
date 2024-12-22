using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Service.Framework.Middlewares;

public class JwtAuthorizationFilter : IActionFilter
{
  public void OnActionExecuting(ActionExecutingContext context)
  {
    // Access JWT from headers
    string jwt = context.HttpContext.Request.Headers["Authorization"];
    if (string.IsNullOrEmpty(jwt))
      // If JWT is not found, return 401 Unauthorized
      context.Result = new UnauthorizedResult();
  }

  public void OnActionExecuted(ActionExecutedContext context)
  {
    // Optional: Additional logic after the action method is executed
  }
}
