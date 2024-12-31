namespace Service.Entities;

public partial class TicketReply
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int? UserId { get; set; }

    public int ContactId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public DateTime Date { get; set; }

    public string? Message { get; set; }

    public int? Attachment { get; set; }

    public int? Admin { get; set; }

    public virtual Staff? AdminNavigation { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
