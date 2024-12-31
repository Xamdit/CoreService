namespace Service.Entities;

public partial class Item
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public string? LongDescription { get; set; }

    public bool Rate { get; set; }

    public int? Tax { get; set; }

    public int? Tax2 { get; set; }

    public string Unit { get; set; } = null!;

    public int? GroupId { get; set; }

    public virtual ItemsGroup? Group { get; set; }
}
