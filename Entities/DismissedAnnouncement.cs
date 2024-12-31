namespace Service.Entities;

public partial class DismissedAnnouncement
{
    public int Id { get; set; }

    public int AnnouncementId { get; set; }

    public bool IsStaff { get; set; }

    public int? UserId { get; set; }

    public DateTime DateRead { get; set; }

    public virtual Announcement Announcement { get; set; } = null!;
}
