using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Controllers.Core;
using Service.Controllers.KnowledgeBase;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.InputSet;

namespace Service.Controllers.KnowledgeBases;

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

  [HttpGet("search")]
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
    var ids = db.KnowledgeBaseGroups.Where(x => x.GroupSlug == slug).Select(x => x.Id).ToList();
    var condition = CreateCondition<Entities.KnowledgeBase>(x => x.ArticleGroup.GroupSlug == slug && ids.Contains(x.ArticleGroupId!.Value));
    data.category = slug;
    data.articles = this.get_all_knowledge_base_articles_grouped(true, condition);
    data.title = data.articles.Count() == 1 ? data.articles[0]["name"] : label("clients_knowledge_base");
    data.knowledge_base_search = true;
    // data(data);
    // self.view("knowledge_base");
    // self.layout();
    return MakeResult(data);
  }

  [HttpPost("add-kb-answer")]
  public IActionResult add_kb_answer([FromBody] KnowledgeBaseArticleAnswerSchema schema)
  {
    var knowledge_base_model = self.knowledge_base_model(db);

    if (!db.is_knowledge_base_viewable()) show_404();
    // This is for did you find self answer useful
    if (self.input.is_ajax_request())
      return MakeResult(new[]
        {
          knowledge_base_model.add_article_answer(schema.article_id, schema.answer)
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
    else if (!db.is_knowledge_base_viewable())
    {
      return show_404();
    }

    return Ok();
  }

  /**
     * Get article by id
     * @param  string $id   article ID
     * @param  string $slug if search by slug
     * @return mixed       if ID or slug passed return object else array
     */
  [HttpGet("get")]
  public IActionResult get([FromQuery] int? group_id = null, [FromQuery] int? id = null, [FromQuery] string slug = "")
  {
    var query = db.KnowledgeBases
      .Include(x => x.ArticleGroup)
      .Include(x => x.KnowedgeBaseArticleFeedbacks)
      .OrderBy(x => x.ArticleOrder)
      .Where(x => x.Id == id);

    if (id.HasValue) query = query.Where(x => x.Id == id);
    if (!string.IsNullOrEmpty(slug)) query = query.Where(x => x.Slug == slug);
    if (self.input.get_has("groupid")) query = query.Where(x => x.ArticleGroupId == group_id.Value);
    return id.HasValue || !string.IsNullOrEmpty(slug)
      ? MakeResult(query.First())
      : MakeResult(query.ToList());
  }
}
