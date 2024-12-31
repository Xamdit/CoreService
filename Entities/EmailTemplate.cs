namespace Service.Entities;

public partial class EmailTemplate
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string FromName { get; set; } = null!;

    public string FromEmail { get; set; } = null!;

    public int PlainText { get; set; }

    public int Active { get; set; }

    public int Order { get; set; }
}
