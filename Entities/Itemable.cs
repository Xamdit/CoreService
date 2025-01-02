namespace Service.Entities;

public partial class Itemable
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? LongDescription { get; set; }

    public double Qty { get; set; }

    public double Rate { get; set; }

    public string Unit { get; set; } = null!;

    public int? ItemOrder { get; set; }

    public virtual ICollection<ItemTax> ItemTaxes { get; set; } = new List<ItemTax>();
}
