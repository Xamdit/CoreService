namespace Service.Entities;

public partial class ActivityLog
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public int? StaffId { get; set; }

    public virtual Staff? Staff { get; set; }
}
