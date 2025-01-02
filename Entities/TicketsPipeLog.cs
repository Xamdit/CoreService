namespace Service.Entities;

public partial class TicketsPipeLog
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public string EmailTo { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Status { get; set; } = null!;
}
