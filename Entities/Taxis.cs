namespace Service.Entities;

public partial class Taxis
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string TaxRate { get; set; } = null!;

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
