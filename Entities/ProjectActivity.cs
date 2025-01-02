namespace Service.Entities;

public partial class ProjectActivity
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int StaffId { get; set; }

    public int ContactId { get; set; }

    public string FullName { get; set; } = null!;

    public bool VisibleToCustomer { get; set; }

    public string DescriptionKey { get; set; } = null!;

    public string? AdditionalData { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
