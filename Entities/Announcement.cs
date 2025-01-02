namespace Service.Entities;

public partial class Announcement
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Message { get; set; }

    public bool ShowToUsers { get; set; }

    public bool ShowToStaff { get; set; }

    public bool ShowName { get; set; }

    public DateTime DateCreated { get; set; }

    public int StaffId { get; set; }

    public virtual ICollection<DismissedAnnouncement> DismissedAnnouncements { get; set; } = new List<DismissedAnnouncement>();

    public virtual Staff Staff { get; set; } = null!;
}
