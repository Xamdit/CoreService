namespace Service.Entities;

public partial class ProjectFile
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public string Subject { get; set; } = null!;

    public string? Description { get; set; }

    public string FileType { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string? LastActivity { get; set; }

    public int ProjectId { get; set; }

    public bool VisibleToCustomer { get; set; }

    public int StaffId { get; set; }

    public int ContactId { get; set; }

    public string External { get; set; } = null!;

    public string? ExternalLink { get; set; }

    public string? ThumbnailLink { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
