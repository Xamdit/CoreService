namespace Service.Entities;

public partial class SalesActivity
{
    public int Id { get; set; }

    public string RelType { get; set; } = null!;

    public int RelId { get; set; }

    public string Description { get; set; } = null!;

    public string? AdditionalData { get; set; }

    public string? StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime Date { get; set; }

    public int? StaffId1 { get; set; }

    public virtual Staff? StaffId1Navigation { get; set; }
}
