namespace Service.Entities;

public partial class TaskChecklistItem
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string Description { get; set; } = null!;

    public bool Finished { get; set; }

    public DateTime DateCreated { get; set; }

    public int AddedFrom { get; set; }

    public int? FinishedFrom { get; set; }

    public string ListOrder { get; set; } = null!;

    public int? Assigned { get; set; }

    public virtual Task Task { get; set; } = null!;
}
