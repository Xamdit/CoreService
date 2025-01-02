namespace Service.Entities;

public partial class TicketsStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDefault { get; set; }

    public string StatusColor { get; set; } = null!;

    public int? StatusOrder { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
