using Task = System.Threading.Tasks.Task;

namespace Service.Framework.Middlewares;

public class FinalMiddlewares(RequestDelegate next)
{
  private void ManageToken(HttpContext context)
  {
    var token = string.Empty;
    if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return;
    var authHeader = authorizationHeader.ToString();
    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
      token = authHeader["Bearer ".Length..].Trim();

    if (!string.IsNullOrEmpty(token))
    {
      var db = MyContext;
      var row = db.Sessions.Where(x => x.Uuid == token).FirstOrDefault();
      if (row != null)
      {
        if (row.ExpiresAt < DateTime.Now)
        {
          Console.WriteLine("Token expired.");
          db.Sessions.Where(x => x.Uuid == token).Delete();
        }
        else
        {
        }
      }
    }
    else
    {
      Console.WriteLine("No JWT found in the request.");
    }
  }

  public async Task InvokeAsync(HttpContext context)
  {
    context.Request.EnableBuffering();
    ManageToken(context);
    self.input.Init(context);
    string soapRequest;
    using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
    {
      soapRequest = await reader.ReadToEndAsync();
      context.Request.Body.Position = 0;
    }

    await next(context);
    if (context.Response.StatusCode == 200)
    {
      var responseStatus = context.Response.StatusCode;
      Console.WriteLine($"Response status: {responseStatus}");
      Console.WriteLine("Middleware executed after the response was sent.");
    }
  }
}
