namespace Service.Entities;

public partial class KnowledgeBase
{
    public int Id { get; set; }

    public int? ArticleGroupId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int Active { get; set; }

    public DateTime DateCreated { get; set; }

    public int ArticleOrder { get; set; }

    public int StaffArticle { get; set; }

    public virtual KnowledgeBaseGroup? ArticleGroup { get; set; }

    public virtual ICollection<KnowedgeBaseArticleFeedback> KnowedgeBaseArticleFeedbacks { get; set; } = new List<KnowedgeBaseArticleFeedback>();
}
