namespace Service.Core.Middlewares;

public class WebMiddleware(RequestDelegate next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    // Check if the user is authenticated
    if (context.User.Identity!.IsAuthenticated)
    {
      // Get username
      var username = context.User.Identity.Name;

      // Get roles
      var roles = context.User.Claims
        .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
        .Select(c => c.Value);

      Console.WriteLine($"Username: {username}");
      foreach (var role in roles) Console.WriteLine($"Role: {role}");
    }
    else
    {
      Console.WriteLine("User is not authenticated.");
    }

    await next(context);
  }
}
