namespace Service.Entities;

public partial class Ticket
{
    public int Id { get; set; }

    public int AdminReplying { get; set; }

    public int? UserId { get; set; }

    public int? ContactId { get; set; }

    public int MergedTicketId { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public int? DepartmentId { get; set; }

    public int? Priority { get; set; }

    public int? Status { get; set; }

    public int? Service { get; set; }

    public string TicketKey { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string? Message { get; set; }

    public int? Admin { get; set; }

    public string Date { get; set; } = null!;

    public int? ProjectId { get; set; }

    public DateTime? LastReply { get; set; }

    public bool ClientRead { get; set; }

    public bool AdminRead { get; set; }

    public int? Assigned { get; set; }

    public int? ResponderId { get; set; }

    public string Cc { get; set; } = null!;

    public virtual Contact? Contact { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Project? Project { get; set; }

    public virtual Staff? Responder { get; set; }

    public virtual TicketsStatus? StatusNavigation { get; set; }

    public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();

    public virtual ICollection<TicketReply> TicketReplies { get; set; } = new List<TicketReply>();
}
