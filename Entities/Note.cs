namespace Service.Entities;

public partial class Note
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DateContacted { get; set; }

    public int AddedFrom { get; set; }

    public DateTime DateCreated { get; set; }
}
