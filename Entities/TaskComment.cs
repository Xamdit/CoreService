namespace Service.Entities;

public partial class TaskComment
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public int TaskId { get; set; }

    public int StaffId { get; set; }

    public int? ContactId { get; set; }

    public int? FileId { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual File? File { get; set; }

    public virtual Staff Staff { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
