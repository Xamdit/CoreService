using System.Net;
using Newtonsoft.Json;
using Service.Framework.Core.Engine;
using Service.Framework.Library.DataStores;
using Service.Framework.Sessions;

namespace Service.Framework.Core.Extensions;

public static class GlobalExtension
{
  public static string globals(this MyInstance self, string key)
  {
    return self.config.get(key);
  }

  public static T? globals<T>(this MyInstance self, string key)
  {
    return (T?)Convert.ChangeType(self.config.get(key), typeof(T));
  }

  public static void log_message(this HelperBase helper, string message)
  {
  }

  public static void log_message(this HelperBase helper, string type, string message)
  {
  }

  public static void log_error(this HelperBase helper, string message)
  {
    Console.WriteLine($"ERROR: {message}"); // Replace with your error logging mechanism
  }

  public static void show_error(this HelperBase helper, string message, HttpStatusCode code = HttpStatusCode.BadRequest)
  {
    var (self, db) = getInstance();
    self.helper.log_message("error", message);
    self.context.Response.StatusCode = (int)code;
    var errorResponse = new { error = message, statusCode = (int)code };
    self.context.Response.ContentType = "application/json";
    self.context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
  }

  public static void ignore(this MyInstance self, Action action)
  {
    try
    {
      action();
    }
    catch (Exception e)
    {
      var instance = MyInstance.Instance;
      Console.WriteLine("Framework.Core.Extensions.GlobalExtension.ignore : " + e.Message);
      self.helper.log_error(e.Message);
    }
  }

  // info : can not set this mimetype from code
  public static string mimetype(string extension, string defaultMimetype = "application/octet-stream")
  {
    var doc = document("./configs/mimetypes.json");
    var row = doc.AsQueryable().FirstOrDefault(x => x.Name == extension);
    return row == null
      ? defaultMimetype
      : $"{row.Value}";
  }

  public static IDocumentCollection<Item> document(string currentPath)
  {
    var folder = Path.GetDirectoryName(currentPath);
    if (!Directory.Exists(folder))
      Directory.CreateDirectory(folder);
    if (!File.Exists(currentPath))
      File.Create(currentPath).Close();
    var store = new DataStore(currentPath);
    var collection = store.GetCollection<Item>();
    return collection;
  }

  /**
 * Set current full url to for user to be redirected after login
 * Check below function to see why is this
 */
  public static void redirect_after_login_to_current_url(this HelperBase helper)
  {
    var (self, db) = getInstance();
    var redirectTo = current_full_url();

    // This can happen if at the time you received a notification but your session was expired the system stored this as last accessed URL so after login can redirect you to this URL.
    // if (strpos($redirectTo, 'notifications_check') !== false) {
    //   return;
    // }

    // get_instance()->session->set_userdata([
    //   'red_url' => $redirectTo,
    //   ]);
  }
}
