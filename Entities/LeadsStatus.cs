namespace Service.Entities;

public partial class LeadsStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? StatusOrder { get; set; }

    public string Color { get; set; } = null!;

    public bool? Lost { get; set; }

    public bool IsDefault { get; set; }

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
