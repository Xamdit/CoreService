namespace Service.Entities;

public partial class Consent
{
    public int Id { get; set; }

    public string Action { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string Ip { get; set; } = null!;

    public int? ContactId { get; set; }

    public int? LeadId { get; set; }

    public string? Description { get; set; }

    public string? OptInPurposeDescription { get; set; }

    public int PurposeId { get; set; }

    public string StaffName { get; set; } = null!;

    public virtual Contact? Contact { get; set; }

    public virtual Lead? Lead { get; set; }

    public virtual ConsentPurpose Purpose { get; set; } = null!;
}
