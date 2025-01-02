namespace Service.Entities;

public partial class WebToLead
{
  public int Id { get; set; }

  public string FormKey { get; set; } = null!;

  public int LeadSource { get; set; }

  public int LeadStatus { get; set; }

  public int NotifyLeadImported { get; set; }

  public string NotifyType { get; set; } = null!;

  public string? NotifyIds { get; set; }

  public int Responsible { get; set; }

  public string Name { get; set; } = null!;

  public string? FormData { get; set; }

  public int Recaptcha { get; set; }

  public string SubmitBtnName { get; set; } = null!;

  public string SubmitBtnTextColor { get; set; } = null!;

  public string SubmitBtnBgColor { get; set; } = null!;

  public string? SuccessSubmitMsg { get; set; }

  public int? SubmitAction { get; set; }

  public string LeadNamePrefix { get; set; } = null!;

  public string? SubmitRedirectUrl { get; set; }

  public string Language { get; set; } = null!;

  public int AllowDuplicate { get; set; }

  public bool MarkPublic { get; set; }

  public string TrackDuplicateField { get; set; } = null!;

  public string TrackDuplicateFieldAnd { get; set; } = null!;

  public int CreateTaskOnDuplicate { get; set; }

  public DateTime DateCreated { get; set; }
}
