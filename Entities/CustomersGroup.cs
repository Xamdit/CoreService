namespace Service.Entities;

public partial class CustomersGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CustomerGroup> CustomerGroups { get; set; } = new List<CustomerGroup>();
}
