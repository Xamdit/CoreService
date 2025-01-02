namespace Service.Entities;

public partial class ScheduledEmail
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    public string Contacts { get; set; } = null!;

    public string? Cc { get; set; }

    public int AttachPdf { get; set; }

    public string Template { get; set; } = null!;
}
