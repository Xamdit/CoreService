namespace Service.Entities;

public partial class TicketAttachment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int? ReplyId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
