namespace Service.Entities;

public partial class Event
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? UserId { get; set; }

    public string Start { get; set; } = null!;

    public string? End { get; set; }

    public int Public { get; set; }

    public string Color { get; set; } = null!;

    public int IsStartNotified { get; set; }

    public int ReminderBefore { get; set; }

    public string ReminderBeforeType { get; set; } = null!;

    public virtual Staff? User { get; set; }
}
