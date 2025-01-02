namespace Service.Entities;

public partial class Taggable
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public int TagId { get; set; }

    public int TagOrder { get; set; }
}
