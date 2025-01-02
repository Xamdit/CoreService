namespace Service.Entities;

public partial class EstimateRequest
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Submission { get; set; } = null!;

    public string? LastStatusChange { get; set; }

    public string? DateEstimated { get; set; }

    public int? FromFormId { get; set; }

    public int? Assigned { get; set; }

    public int? Status { get; set; }

    public int DefaultLanguage { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual EstimateRequestForm? FromForm { get; set; }
}
