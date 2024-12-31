namespace Service.Entities;

public partial class TaskAssigned
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int TaskId { get; set; }

    public int AssignedFrom { get; set; }

    public int IsAssignedFromContact { get; set; }

    public virtual Staff Staff { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
