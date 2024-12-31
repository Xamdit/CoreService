namespace Service.Entities;

public partial class Reminder
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public int IsNotified { get; set; }

    public int RelId { get; set; }

    public int? StaffId { get; set; }

    public string RelType { get; set; } = null!;

    public int NotifyByEmail { get; set; }

    public int Creator { get; set; }

    public virtual Staff? Staff { get; set; }
}
