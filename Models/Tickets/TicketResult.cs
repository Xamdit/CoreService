using Global.Entities;

namespace Service.Models.Tickets;

public class TicketResult
{
  public string Submiter { get; set; } = string.Empty;
  public Ticket Ticket { get; set; }
  public string OpenedBy { get; set; } = string.Empty;
}
