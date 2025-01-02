namespace Service.Entities;

public partial class ItemTax
{
    public int Id { get; set; }

    public int? ItemId { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public decimal TaxRate { get; set; }

    public string TaxName { get; set; } = null!;

    public virtual Itemable? Item { get; set; }
}
