using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;

namespace Service.Controllers;

[ApiController]
[Route("api/knowledge-base")]
public class KnowledgeBaseController(ILogger<KnowledgeBaseController> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  public override void Init()
  {
    if (db.is_staff_logged_in() && db.get_option_compare("use_knowledge_base", 0))
      set_alert("warning", "Knowledge base is disabled, navigate to Setup.Settings.Customers and set Use Knowledge Base to YES.");
    hooks.do_action("customers_area_knowledge_base_construct");
  }

  [HttpGet]
  public IActionResult index(string slug = "")
  {
    checkKnowledgeBaseAccess();
    data.articles = this.get_all_knowledge_base_articles_grouped(true);
    data.knowledge_base_search = true;
    data.title = label("clients_knowledge_base");
    // self.view("knowledge_base");
    // data(data);
    // self.layout();
    return MakeResult(data);
  }

  public IActionResult search()
  {
    checkKnowledgeBaseAccess();
    var q = self.input.get("q");
    var where_kb = [];
    if (!string.IsNullOrEmpty(q))
      where_kb = "(subject LIKE '%{q}%' OR description LIKE '%{q}%' OR slug LIKE '%{q}%')";

    data.articles = this.get_all_knowledge_base_articles_grouped(true, where_kb);
    data.search_results = true;
    data.title = label("showing_search_result", q);
    data.knowledge_base_search = true;
    // self.view("knowledge_base");
    // data(data);
    // self.layout();
    return MakeResult(data);
  }

  public IActionResult article(string slug)
  {
    var knowledge_base_model = self.knowledge_base_model(db);
    checkKnowledgeBaseAccess();
    data.article = knowledge_base_model.get(false, slug);

    if (!string.IsNullOrEmpty(slug))
      return Redirect(site_url("knowledge-base"));

    if (!data.article || data.article.active_article == 0) show_404();

    data.knowledge_base_search = true;
    data.related_articles = knowledge_base_model.get_related_articles(data.article.articleid);
    db.add_views_tracking("kb_article", data.article.articleid);
    data.title = data.article.subject;
    // self.view("knowledge_base_article");
    // data(data);
    // self.layout();
    return MakeResult(data);
  }

  public IActionResult category(string slug)
  {
    checkKnowledgeBaseAccess();
    var where_kb = $"articlegroup IN (SELECT groupid FROM knowledge_base_groups WHERE group_slug='{slug}')";
    data.category = slug;
    data.articles = this.get_all_knowledge_base_articles_grouped(true, where_kb);

    data.title = data.articles.Count() == 1 ? data.articles[0]["name"] : label("clients_knowledge_base");
    data.knowledge_base_search = true;
    // data(data);
    // self.view("knowledge_base");
    // self.layout();
    return MakeResult(data);
  }

  [HttpPost("add-kb-answer")]
  public IActionResult add_kb_answer()
  {
    var knowledge_base_model = self.knowledge_base_model(db);

    if (!is_knowledge_base_viewable()) show_404();
    // This is for did you find self answer useful
    if (self.input.is_ajax_request())
      return MakeResult(new[]
        {
          knowledge_base_model.add_article_answer(self.input.post("articleid"), self.input.post("answer")
        }
      );

    return Ok();
  }

  private IActionResult checkKnowledgeBaseAccess()
  {
    if (db.get_option_compare("use_knowledge_base", 1) && !db.is_client_logged_in() && db.get_option_compare("knowledge_base_without_registration", 0))
    {
      if (db.is_staff_logged_in())
      {
        set_alert(
          "warning",
          "Knowledge base is available only for logged in contacts, you are accessing self page as staff member only for preview."
        );
      }
      else
      {
        // Knowedge base viewable only for registered customers
        // Redirect to login page so they can login to view
        redirect_after_login_to_current_url();
        return Redirect(site_url("authentication/login"));
      }
    }
    else if (!is_knowledge_base_viewable())
    {
      return show_404();
    }

    return Ok();
  }
}
