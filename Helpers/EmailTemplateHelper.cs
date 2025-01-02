using System.Reflection;
using Newtonsoft.Json;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Framework.Library.MergeFields;
using Service.Framework.Library.Merger;
using Service.Models;

namespace Service.Helpers;

public static class EmailTemplateHelper
{
  /**
* Send mail template
* @since  2.3.0
* @return mixed
*/
  public static bool send_mail_template(this MyContext db, params object[] args)
  {
    // $params = func_get_args();
    // return mail_template(...$params).send();
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
  public static EmailsModel mail_template(this MyModel model, string className, params object[] parameters)
  {
    var (self, db) = model.getInstance();
    // Get the path of the mail template class file
    var path = GetMailTemplatePath(className, parameters);

    if (file_exists(path))
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
    var instance = self.emails_model(db);

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


  /**
 * Parse email template with the merge fields
 * @param  mixed  template     template
 * @param  array  $merge_fields
 * @return object
 */
  public static EmailTemplate parse_email_template(this AppControllerBase controller, EmailTemplate template, Dictionary<string, string> merge_fields = default)
  {
    var (self, db) = controller.getInstance();
    if (string.IsNullOrEmpty(template.Name) || self.input.post_has("template_name"))
    {
      var original_template = template;
      var emails_model = self.emails_model(db);
      if (self.input.post_has("template_name"))
        template.Name = self.input.post("template_name");

      var row = emails_model.get(x => x.Slug == template.Slug).First();
      if (self.input.post_has("email_template_custom"))
      {
        row.Message = self.input.post<string>("email_template_custom");
        // Replace the subject too
        row.Subject = original_template.Subject;
      }
    }

    template = parse_email_template_merge_fields(template, merge_fields);

    // Used in hooks eq for emails tracking
    // template.tmp_id = uuid();

    return hooks.apply_filters("email_template_parsed", template);
  }

  /**
 * This function will parse email template merge fields and replace with the corresponding merge fields passed before sending email
 * @param  object $template     template from database
 * @param  array $merge_fields available merge fields
 * @return object
 */
  public static EmailTemplate? parse_email_template_merge_fields(EmailTemplate template, Dictionary<string, string> merge_fields)
  {
    var other_merge_fields = self.library.other_merge_fields(AppGlobal.ServiceProvider);
    merge_fields = (Dictionary<string, string>)TypeMerger.Merge(merge_fields, other_merge_fields.format());
    var template_checker = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(template));
    var temp = new Dictionary<string, object>();
    foreach (var key in merge_fields.Keys.ToList())
    {
      var items = new List<string>() { "message", "fromname", "subject" };
      var val = merge_fields[key];
      foreach (var replacer in items)
        temp[replacer] = Convert.ToString(template_checker[replacer]).Contains(key)
          ? Convert.ToString(template_checker[replacer])!.Replace(key, val)
          : Convert.ToString(template_checker[replacer])!.Replace(key, "");
    }

    return JsonConvert.DeserializeObject<EmailTemplate>(JsonConvert.SerializeObject(temp));
  }
}
