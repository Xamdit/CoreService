namespace Service.Entities;

public partial class StaffPermission
{
    public int Id { get; set; }

    public int? StaffId { get; set; }

    public string Feature { get; set; } = null!;

    public string Capability { get; set; } = null!;

    public virtual Staff? Staff { get; set; }
}
