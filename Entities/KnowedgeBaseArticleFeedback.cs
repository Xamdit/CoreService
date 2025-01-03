namespace Service.Entities;

public partial class KnowedgeBaseArticleFeedback
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public bool Answer { get; set; }

    public string Ip { get; set; } = null!;

    public DateTime Date { get; set; }

    public virtual KnowledgeBase Article { get; set; } = null!;
}
