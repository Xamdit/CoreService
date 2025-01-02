namespace Service.Framework.Library.Email;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

public class AppMailer
{
  private SmtpClient _smtpClient;
  private MailMessage _mailMessage;
  private bool _enableDebug;
  private List<string> _debugOutput;

  public AppMailer()
  {
    _smtpClient = new SmtpClient();
    _mailMessage = new MailMessage();
    _debugOutput = new List<string>();
  }

  public void initialize(string smtpHost, int smtpPort, string smtpUser, string smtpPass, bool enableSsl)
  {
    _smtpClient.Host = smtpHost;
    _smtpClient.Port = smtpPort;
    _smtpClient.EnableSsl = enableSsl;
    _smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);

    log_debug($"SMTP Initialized: Host={smtpHost}, Port={smtpPort}, SSL={enableSsl}");
  }

  public void set_debug_output(Func<object, object> func)
  {
    _enableDebug = true;
  }

  public void set_smtp_debug(int level)
  {
    // SMTP debug levels are informational. Here we just store them.
    log_debug($"SMTP Debug Level Set: {level}");
  }

  public void set_newline(string newline)
  {
    _mailMessage.BodyEncoding = newline == "\r\n" ? System.Text.Encoding.UTF8 : System.Text.Encoding.ASCII;
  }

  public void set_crlf(string crlf)
  {
    // Placeholder for handling CRLF settings, can be expanded as needed
    log_debug($"CRLF Set: {crlf}");
  }

  public void from(string email, string displayName = null)
  {
    _mailMessage.From = new MailAddress(email, displayName);
    log_debug($"From Address Set: {email}, DisplayName={displayName}");
  }

  public void to(string email)
  {
    _mailMessage.To.Add(email);
    log_debug($"To Address Added: {email}");
  }

  public void bcc(string email)
  {
    _mailMessage.Bcc.Add(email);
    log_debug($"BCC Address Added: {email}");
  }

  public void subject(string subject)
  {
    _mailMessage.Subject = subject;
    log_debug($"Subject Set: {subject}");
  }

  public void message(string body, bool isHtml = true)
  {
    _mailMessage.Body = body;
    _mailMessage.IsBodyHtml = isHtml;
    log_debug($"Message Set: IsHtml={isHtml}");
  }

  public bool send(bool throwOnError = false)
  {
    try
    {
      _smtpClient.Send(_mailMessage);
      log_debug("Email Sent Successfully.");
      return true;
    }
    catch (Exception ex)
    {
      log_debug($"Email Sending Failed: {ex.Message}");
      if (throwOnError) throw;

      return false;
    }
  }

  public string print_debugger()
  {
    return string.Join(Environment.NewLine, _debugOutput);
  }

  private void log_debug(string message)
  {
    if (_enableDebug) _debugOutput.Add($"[DEBUG] {message}");
  }
}

public class AppMailerTestUnit
{
  public void test()
  {
    var mailer = new AppMailer();
    mailer.initialize("smtp.example.com", 587, "user@example.com", "password", true);
    mailer.set_debug_output(obj =>
    {
      Console.WriteLine(obj);
      return null;
    });
    mailer.from("no-reply@example.com", "Example App");
    mailer.to("recipient@example.com");
    mailer.bcc("bcc@example.com");
    mailer.subject("Test Email");
    mailer.message("<h1>Hello, World!</h1>", true);
    Console.WriteLine(mailer.send() ? "Email sent successfully!" : "Failed to send email.");
    Console.WriteLine(mailer.print_debugger());
  }
}
