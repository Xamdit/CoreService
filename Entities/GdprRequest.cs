namespace Service.Entities;

public partial class GdprRequest
{
    public int Id { get; set; }

    public int? ClientId { get; set; }

    public int? ContactId { get; set; }

    public int? LeadId { get; set; }

    public string RequestType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string RequestDate { get; set; } = null!;

    public string RequestFrom { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual Lead? Lead { get; set; }
}
