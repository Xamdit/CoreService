using Global.Entities;

namespace Service.Models.Tickets;

public class TicketOption : Ticket
{
  public List<CustomField> custom_fields { get; set; } = new();
  public List<Taggable> Tags { get; set; } = new();
  public string Subject { get; set; } = string.Empty;
  public int MergedTicketId = 0;
}
