namespace Service.Entities;

public partial class EstimateRequestForm
{
    public int Id { get; set; }

    public string FormKey { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? FormData { get; set; }

    public int? Recaptcha { get; set; }

    public int Status { get; set; }

    public string SubmitBtnName { get; set; } = null!;

    public string SubmitBtnBgColor { get; set; } = null!;

    public string SubmitBtnTextColor { get; set; } = null!;

    public string? SuccessSubmitMsg { get; set; }

    public int? SubmitAction { get; set; }

    public string? SubmitRedirectUrl { get; set; }

    public string Language { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string NotifyType { get; set; } = null!;

    public string? NotifyIds { get; set; }

    public int? Responsible { get; set; }

    public int NotifyRequestSubmitted { get; set; }

    public virtual ICollection<EstimateRequest> EstimateRequests { get; set; } = new List<EstimateRequest>();
}
