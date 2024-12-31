namespace Service.Entities;

public partial class Milestone
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DescriptionVisibleToCustomer { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int ProjectId { get; set; }

    public string Color { get; set; } = null!;

    public int MilestoneOrder { get; set; }

    public DateTime DateCreated { get; set; }

    public int? HideFromCustomer { get; set; }

    public virtual Project Project { get; set; } = null!;
}
