using Service.Entities;

namespace Service.Models.Tickets;

public class TicketReplyDto : TicketReply
{
  public string Submiter { get; set; } = string.Empty;
  public string FromName { get; set; } = string.Empty;
  public List<TicketAttachment> Attachments { get; set; } = new();
}
