namespace Service.Entities;

public partial class EstimateRequestStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? StatusOrder { get; set; }

    public string Color { get; set; } = null!;

    public string Flag { get; set; } = null!;
}
