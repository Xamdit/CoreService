namespace Service.Entities;

public partial class ProjectNote
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Content { get; set; } = null!;

    public int StaffId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
