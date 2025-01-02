namespace Service.Entities;

public partial class KnowledgeBaseGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? GroupSlug { get; set; }

    public string? Description { get; set; }

    public int Active { get; set; }

    public string Color { get; set; } = null!;

    public int? GroupOrder { get; set; }

    public virtual ICollection<KnowledgeBase> KnowledgeBases { get; set; } = new List<KnowledgeBase>();
}
