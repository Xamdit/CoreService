namespace Service.Entities;

public partial class LeadActivityLog
{
    public int Id { get; set; }

    public int? LeadId { get; set; }

    public string Description { get; set; } = null!;

    public string? AdditionalData { get; set; }

    public DateTime Date { get; set; }

    public int StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public int CustomActivity { get; set; }

    public virtual Lead? Lead { get; set; }
}
