namespace Service.Entities;

public partial class ProjectMember
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int StaffId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
