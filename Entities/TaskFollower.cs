namespace Service.Entities;

public partial class TaskFollower
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int TaskId { get; set; }

    public virtual Staff Staff { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
