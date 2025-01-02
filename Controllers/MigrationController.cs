using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;
using Service.Framework.Helpers;
using Service.Helpers.Database;
using SqlKata.Execution;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationController(ILogger<MigrationController> logger, MyInstance self,MyContext db) : ClientControllerBase(logger, self,db)
{
  [HttpGet("make")]
  public IActionResult Make()
  {
    self.config.Load("migration");
    if (self.config.item<bool>("migration_enabled") != true)
      return Content(
        "<h1>Set config item <b>migration_enabled</b> to TRUE in <b>application/config/migration.php</b></h1>",
        "text/html");

    var oldBaseUrl = self.input.get("old_base_url");
    if (string.IsNullOrEmpty(oldBaseUrl))
      return Content(
        $"<h1>You need to pass the old base URL in the URL like: {self.helper.site_url("migration/make?old_base_url=http://myoldbaseurl.com/")}</h1>",
        "text/html");

    var newBaseUrl = self.config.item<string>("base_url");
    if (!oldBaseUrl.EndsWith("/")) oldBaseUrl += "/";

    var tables = new[]
    {
      new { table = "notifications", field = "description" },
      new { table = "notifications", field = "additional_data" },
      new { table = "notes", field = "description" },
      new { table = "emailtemplates", field = "message" },
      new { table = "newsfeed_posts", field = "content" },
      new { table = "newsfeed_post_comments", field = "content" },
      new { table = "options", field = "value" },
      new { table = "staff", field = "email_signature" },
      new { table = "tickets_predefined_replies", field = "message" },
      new { table = "projectdiscussioncomments", field = "content" },
      new { table = "projectdiscussions", field = "description" },
      new { table = "project_notes", field = "content" },
      new { table = "projects", field = "description" },
      new { table = "reminders", field = "description" },
      new { table = "tasks", field = "description" },
      new { table = "task_comments", field = "content" },
      new { table = "ticket_replies", field = "message" },
      new { table = "tickets", field = "message" },
      new { table = "todos", field = "description" },
      new { table = "proposal_comments", field = "content" },
      new { table = "proposals", field = "content" },
      new { table = "lead_activity_log", field = "description" },
      new { table = "knowledge_base_groups", field = "description" },
      new { table = "knowledge_base", field = "description" },
      new { table = "invoices", field = "terms" },
      new { table = "invoices", field = "clientnote" },
      new { table = "invoices", field = "adminnote" },
      new { table = "creditnotes", field = "terms" },
      new { table = "creditnotes", field = "clientnote" },
      new { table = "creditnotes", field = "adminnote" },
      new { table = "milestones", field = "description" },
      new { table = "sales_activity", field = "description" },
      new { table = "sales_activity", field = "additional_data" },
      new { table = "estimates", field = "terms" },
      new { table = "estimates", field = "clientnote" },
      new { table = "estimates", field = "adminnote" },
      new { table = "contracts", field = "description" },
      new { table = "contract_comments", field = "content" },
      new { table = "contracts", field = "content" },
      new { table = "activity_log", field = "description" },
      new { table = "announcements", field = "message" },
      new { table = "consent_purposes", field = "description" },
      new { table = "consents", field = "description" },
      new { table = "consents", field = "opt_in_purpose_description" },
      new { table = "vault", field = "description" }
    };

    tables = hooks.apply_filters("migration_tables_to_replace_old_links", tables);

    var affectedRows = 0;

    foreach (var t in tables)
    {
      // var query = $"UPDATE `{t.table}` SET `{t.field}` = REPLACE({t.field}, @oldBaseUrl, @newBaseUrl)";
      // db.query(query, new { oldBaseUrl, newBaseUrl });
      var kata = self.db.kata(t.table);
      // affectedRows += kata
      //   .Update(t.field, kata.Raw($"REPLACE({t.field}, @oldBaseUrl, @newBaseUrl)"),
      //     new { oldBaseUrl, newBaseUrl });

      var affected = kata.Where("Id", 1).Update(new
      {
        Price = 18,
        Status = "active"
      });
    }

    return Content($"<h1>Total links replaced: {affectedRows}</h1>", "text/html");
  }
}
