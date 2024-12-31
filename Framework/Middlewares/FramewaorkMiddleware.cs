using Service.Framework.Core.InputSet;

namespace Service.Framework.Middlewares;

public class FinalMiddlewares(RequestDelegate next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    context.Request.EnableBuffering();
    var (self, db) = getInstance();
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
      // var aismember = new AisMember();
      // aismember.MtSender(soapRequest);
      Console.WriteLine("Middleware executed after the response was sent.");
    }
  }
}
