namespace Service.Entities;

public partial class MailQueue
{
    public int Id { get; set; }

    public string Engine { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Cc { get; set; }

    public string? Bcc { get; set; }

    public string Message { get; set; } = null!;

    public string? AltMessage { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? Date { get; set; }

    public string? Headers { get; set; }

    public string? Attachments { get; set; }
}
