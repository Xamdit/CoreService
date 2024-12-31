namespace Service.Entities;

public partial class TasksTimer
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int StaffId { get; set; }

    public bool HourlyRate { get; set; }

    public string? Note { get; set; }

    public virtual Staff Staff { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
