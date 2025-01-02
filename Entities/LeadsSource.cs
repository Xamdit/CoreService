namespace Service.Entities;

public partial class LeadsSource
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
