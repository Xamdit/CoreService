using System.Reflection;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;
using Service.Models;

namespace Service.Helpers;

public static class EmailTemplateHelper
{
  /**
* Send mail template
* @since  2.3.0
* @return mixed
*/
  public static bool send_mail_template(this HelperBase helper, params object[] args)
  {
    // $params = func_get_args();
    // return mail_template(...$params)->send();
    return true;
  }

  public static void delete_tracked_emails(int rel_id, string rel_type)
  {
    var db = new MyContext();
    db.TrackedMails.Where(x => x.RelId == rel_id && x.RelType == rel_type).Delete();
  }

  /**
 * Prepare mail template class
 * @param  string $class mail template class name
 * @return mixed
 */
  public static EmailsModel mail_template(string className, params object[] parameters)
  {
    var (self, db) = getInstance();
    // Get the path of the mail template class file
    var path = GetMailTemplatePath(className, parameters);

    if (self.helper.file_exists(path))
    {
      // Handle error for non-existent mail template
      if (!IsCronJob())
        throw new FileNotFoundException($"Mail Class Does Not Exist [{path}]");
      else
        return null; // In a cron job, we just return null
    }

    // Load the assembly (assuming the mail template class is in an external assembly)
    var mailTemplateAssembly = Assembly.LoadFrom(path);

    // Get the type of the class dynamically
    var mailTemplateType = mailTemplateAssembly.GetType(className);
    if (mailTemplateType == null) throw new TypeLoadException($"Mail class '{className}' not found in the assembly.");

    // Initialize the class with the provided parameters
    // var instance = Activator.CreateInstance(mailTemplateType, parameters);
    var instance = self.model.emails_model();

    // Check if the class has a Send method or some equivalent functionality
    var sendMethod = mailTemplateType.GetMethod("Send");
    if (sendMethod == null) throw new MissingMethodException($"Mail class '{className}' does not contain a 'Send' method.");

    // Optionally call the send method or return the instance
    sendMethod.Invoke(instance, null);

    return instance;
  }

  private static string GetMailTemplatePath(string className, object[] parameters)
  {
    // Logic to construct the file path for the mail template class
    // For example: assuming the class is compiled in an assembly
    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MailTemplates", $"{className}.dll");
  }

  private static bool IsCronJob()
  {
    // Logic to check if this is a cron job
    // This is an approximation, you can implement based on your environment
    return Environment.GetEnvironmentVariable("IS_CRON_JOB") == "true";
  }
}
