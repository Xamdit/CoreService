namespace Service.Entities;

public partial class RelatedItem
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public int ItemId { get; set; }
}
