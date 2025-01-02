namespace Service.Entities;

public partial class TrackedMail
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Opened { get; set; }

    public DateTime? DateOpened { get; set; }

    public string? Subject { get; set; }
}
