namespace Service.Entities;

public partial class File
{
    public int Id { get; set; }

    public string Uuid { get; set; } = null!;

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public bool VisibleToCustomer { get; set; }

    public string AttachmentKey { get; set; } = null!;

    public string External { get; set; } = null!;

    public string? ExternalLink { get; set; }

    public string? ThumbnailLink { get; set; }

    public int? StaffId { get; set; }

    public int? ContactId { get; set; }

    public int? TaskCommentId { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual ICollection<SharedCustomerFile> SharedCustomerFiles { get; set; } = new List<SharedCustomerFile>();

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
}
