using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework;
using Task = System.Threading.Tasks.Task;

namespace Service.Models.KnowedgeBases;

public class KnowledgeBaseModel(MyInstance self, MyContext db) : MyModel(self)
{
  // Get article by id or slug
  public KnowledgeBase? get_article(int? id = null, string slug = null)
  {
    var query = db.KnowledgeBases
      .Include(k => k.ArticleGroup)
      .AsQueryable();

    if (id.HasValue) query = query.Where(k => k.ArticleGroup.Id == id.Value);
    if (!string.IsNullOrEmpty(slug)) query = query.Where(k => k.Slug == slug);
    // if (int.TryParse(HttpContext.Current.Request.QueryString["groupid"], out int groupId)) query = query.Where(k => k.ArticleGroupId == groupId);
    return query
      .OrderBy(k => k.ArticleOrder)
      .FirstOrDefault();
  }

  // Get related articles based on article id
  public async Task<List<KnowledgeBase>> GetRelatedArticles(int currentId, bool customers = true)
  {
    var article = db.KnowedgeBaseArticleFeedbacks
      // .Select(k => k.ArticleGroupId)
      .FirstOrDefault(k => k.ArticleId == currentId);

    return db.KnowledgeBases
      // .Where(k => k.ArticleGroupId == article && k.ArticleId != currentId && k.Active)
      .Where(k => k.ArticleGroupId == article.Id && k.ArticleGroupId != currentId && k.Active == 1)
      .Where(k => customers ? k.StaffArticle == 0 : k.StaffArticle == 1)
      .Take(5) // Adjust total related articles here
      .ToList();
  }

  // Add new article
  public int add_article(KnowledgeBase article)
  {
    // article.Active = !article.Disabled;
    // article.StaffArticle = article.StaffArticle ? 1 : 0;
    article.DateCreated = DateTime.UtcNow;
    article.Slug = GenerateSlug(article.Subject);

    var existingSlugCount = db.KnowledgeBases.Count(k => k.Slug.StartsWith(article.Slug));
    if (existingSlugCount > 0) article.Slug += $"-{existingSlugCount + 1}";

    db.KnowledgeBases.Add(article);

    log_activity($"New Article Added [ArticleID: {article.Id} GroupID: {article.ArticleGroupId}]");

    return article.Id;
  }

  // Update article
  public async Task<bool> UpdateArticle(KnowedgeBaseArticleFeedback article, int id)
  {
    var existingArticle = db.KnowledgeBases.FirstOrDefault(x => x.Id == id);
    if (existingArticle == null) return false;
    // existingArticle.Active = !article.Disabled;
    // existingArticle.StaffArticle = article.StaffArticle ? 1 : 0;
    // existingArticle.Subject = article.Subject;
    // existingArticle.Description = article.Description;
    // existingArticle.ArticleGroupId = article.ArticleGroupId;

    db.KnowledgeBases.Update(existingArticle);
    log_activity($"Article Updated [ArticleID: {id}]");
    return true;
  }

  // Update kanban order
  public async Task<bool> UpdateKanban(KanbanData data)
  {
    var affectedRows = 0;
    foreach (var order in data.Order)
    {
      var article = db.KnowledgeBases.Find(order[0]);
      if (article == null) continue;
      article.ArticleOrder = order[1];
      article.ArticleGroupId = data.GroupId;
      db.KnowledgeBases.Update(article);
      affectedRows++;
    }

    return affectedRows > 0;
  }

  // Change article status
  public async Task ChangeArticleStatus(int id, int status)
  {
    var article = db.KnowledgeBases.FirstOrDefault(x => x.Id == id);
    if (article != null)
    {
      article.Active = status;
      db.KnowledgeBases.Update(article);
      log_activity($"Article Status Changed [ArticleID: {id} Status: {status}]");
    }
  }

  // Get all knowledge base groups
  public KnowledgeBaseGroup? GetKnowledgeBaseGroup(int id)
  {
    var row = db.KnowledgeBaseGroups
      .Include(x => x.KnowledgeBases)
      .FirstOrDefault(x => x.Id == id);
    return row;
  }

  public List<KnowledgeBaseGroup> GetKnowledgeBaseGroups(int? active = null)
  {
    var query = db.KnowledgeBaseGroups.AsQueryable();
    if (active.HasValue) query = query.Where(k => k.Active == active.Value);
    return query.OrderBy(g => g.GroupOrder).ToList();
  }

  // Add new knowledge base group
  public int AddGroup(KnowledgeBaseGroup group)
  {
    // group.Active = !group.Disabled;
    group.GroupSlug = GenerateSlug(group.Name);

    var existingSlugCount = db.KnowledgeBaseGroups.Count(g => g.GroupSlug.StartsWith(group.GroupSlug));
    if (existingSlugCount > 0) group.GroupSlug += $"-{existingSlugCount + 1}";
    db.KnowledgeBaseGroups.Add(group);
    log_activity($"New Article Group Added [GroupID: {group.Id}]");
    return group.Id;
  }

  // Update knowledge base group
  public async Task<bool> UpdateGroup(KnowledgeBaseGroup group, int id)
  {
    var existingGroup = db.KnowledgeBaseGroups.FirstOrDefault(x => x.Id == id);
    if (existingGroup == null) return false;

    // existingGroup.Active = !group.Disabled;
    existingGroup.Name = group.Name;
    existingGroup.GroupSlug = group.GroupSlug;

    db.KnowledgeBaseGroups.Update(existingGroup);

    log_activity($"Article Group Updated [GroupID: {id}]");

    return true;
  }

  // Change group status
  public async Task ChangeGroupStatus(int id, int status)
  {
    var group = db.KnowledgeBaseGroups.FirstOrDefault(x => x.Id == id);
    if (group != null)
    {
      group.Active = status;
      db.KnowledgeBaseGroups.Update(group);
      log_activity($"Article Status Changed [GroupID: {id} Status: {status}]");
    }
  }

  // Delete article
  public async Task<bool> DeleteArticle(int id)
  {
    var article = db.KnowledgeBases.FirstOrDefault(x => x.Id == id);
    if (article == null) return false;
    db.KnowledgeBases.Remove(article);
    log_activity($"Article Deleted [ArticleID: {id}]");
    return true;
  }

  // Other methods...

  private string GenerateSlug(string subject)
  {
    // Implement your slug generation logic here
    return subject.ToLower().Replace(" ", "-");
  }
}
