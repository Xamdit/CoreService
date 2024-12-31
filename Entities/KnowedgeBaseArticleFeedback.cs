namespace Service.Entities;

public partial class KnowedgeBaseArticleFeedback
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public int Answer { get; set; }

    public string Ip { get; set; } = null!;

    public string Date { get; set; } = null!;

    public virtual KnowledgeBase Article { get; set; } = null!;
}
