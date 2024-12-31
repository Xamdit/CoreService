namespace Service.Entities;

public partial class ProjectDiscussionComment
{
    public int Id { get; set; }

    public int DiscussionId { get; set; }

    public string DiscussionType { get; set; } = null!;

    public int? Parent { get; set; }

    public DateTime DateCreated { get; set; }

    public string? Modified { get; set; }

    public string Content { get; set; } = null!;

    public int StaffId { get; set; }

    public int ContactId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileMimeType { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public virtual Contact Contact { get; set; } = null!;

    public virtual ProjectDiscussion Discussion { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
