using Global.Entities;

namespace Service.Models.Tickets;

public class TicketDto
{
  public TicketAttachment attachment { get; set; } = new();
  public string data { get; set; } = string.Empty;
}
