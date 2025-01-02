using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using Service.Entities;
using Service.Framework;
using Service.Models.Contracts;
using File = Service.Entities.File;

namespace Service.Models;

public class EmailsModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  private readonly List<MailAttachment> _attachments = new();


  public IEnumerable<EmailTemplate> get(Expression<Func<EmailTemplate, bool>> condition)
  {
    var query = db.EmailTemplates.AsQueryable();
    query = query.Where(condition);
    return query.ToList();
  }

  /**
 * Set template merge fields
 * @param array $fields
 */
  public EmailsModel set_merge_fields(CustomField fields, params object[] args)
  {
    // if (!is_array(fields)) fields = app_merge_fields.format_feature(fields, args);
    // var merge_fields = array_merge(this.merge_fields, fields);

    return this;
  }

  public EmailTemplate? get_email_template_by_id(int id)
  {
    var row = db.EmailTemplates.FirstOrDefault(x => x.Id == id);
    return row;
  }

  public int? add_template(EmailTemplate template)
  {
    db.EmailTemplates.Add(template);

    return template.Id;
  }

  public bool update(Dictionary<int, EmailTemplate> templatesData)
  {
    var affectedRows = 0;
    foreach (var entry in templatesData)
    {
      var template = db.EmailTemplates.Find(entry.Key);
      if (template == null) continue;
      template.Subject = entry.Value.Subject;
      template.FromName = entry.Value.FromName;
      template.FromEmail = entry.Value.FromEmail ?? string.Empty;
      template.Message = entry.Value.Message;
      template.PlainText = entry.Value.PlainText;
      template.Active = entry.Value.Active;
      db.EmailTemplates.Update(template);
      affectedRows += db.SaveChanges();
    }

    return affectedRows > 0;
  }

  public bool MarkAs(string slug, bool enabled)
  {
    var template = db.EmailTemplates.FirstOrDefault(t => t.Slug == slug);
    if (template == null) return false;
    template.Active = enabled ? 1 : 0;

    return true;
  }

  public bool MarkAsByType(string type, bool enabled)
  {
    var templates = db.EmailTemplates.Where(t => t.Type == type && t.Slug != "two-factor-authentication");
    foreach (var template in templates)
      template.Active = enabled ? 1 : 0;

    return true;
  }

  public bool SendSimpleEmail(string email, string subject, string message)
  {
    var mailConfig = new
    {
      FromEmail = "smtp_email@example.com", // Replace with actual SMTP email option
      FromName = "Company Name",
      Email = email,
      Subject = subject,
      Message = message
    };

    var mailTemplate = new EmailTemplate
    {
      Message = GetEmailHeader() + mailConfig.Message + GetEmailFooter(),
      FromName = mailConfig.FromName,
      Subject = mailConfig.Subject
    };

    var parsedTemplate = ParseEmailTemplate(mailTemplate);

    // mailConfig = new
    // {
    //   FromEmail = mailConfig.FromEmail,
    //   Email = mailConfig.Email,
    //   Subject = mailConfig.Subject,
    //   Message = parsedTemplate.Message,
    //   FromName = parsedTemplate.FromName
    // };

    return SendEmail(mailConfig.Email, mailConfig.Subject, mailConfig.Message, mailConfig.FromEmail, mailConfig.FromName);
  }

  private bool SendEmail(string to, string subject, string body, string fromEmail, string fromName)
  {
    try
    {
      using var smtpClient = new SmtpClient("smtp.example.com") // Replace with actual SMTP server
      {
        Port = 587,
        Credentials = new NetworkCredential("username", "password"), // Replace with actual credentials
        EnableSsl = true
      };
      var mailMessage = new MailMessage
      {
        From = new MailAddress(fromEmail, fromName),
        Subject = subject,
        Body = body,
        IsBodyHtml = true // Assuming the email message is HTML, change as needed
      };
      mailMessage.To.Add(to);

      var items = _attachments.Select(x => new Attachment(x.filename)).ToList();
      foreach (var attachment in items) mailMessage.Attachments.Add(attachment);
      smtpClient.Send(mailMessage);
      log_activity($"Email sent to: {to} Subject: {subject}");
      ClearAttachments();
      return true;
    }
    catch (Exception ex)
    {
      log_activity($"Failed to send email: {ex.Message}");
      return false;
    }
  }

  private string GetEmailHeader()
  {
    return "<h1>Email Header</h1>";
    // Replace with actual header
  }

  private string GetEmailFooter()
  {
    return "<footer>Email Footer</footer>";
    // Replace with actual footer
  }


  private void ClearAttachments()
  {
    _attachments.Clear();
  }

  private EmailTemplate ParseEmailTemplate(EmailTemplate template)
  {
    // Logic to parse email templates
    return template;
  }

  public bool send()
  {
    return false;
  }


  /**
* @param resource
* @param string
* @param string (mime type)
* @return none
* Add attachment to property to check before an email is send
*/
  public void add_attachment(MailAttachment attachment)
  {
    _attachments.Add(attachment);
  }

  public void add_attachment(File attachment)
  {
    //_attachments.Add(attachment);
  }

  public Dictionary<string, string> get_merge_fields()
  {
    return new Dictionary<string, string>();
  }
}
