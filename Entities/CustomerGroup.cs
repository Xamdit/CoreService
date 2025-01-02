namespace Service.Entities;

public partial class CustomerGroup
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int CustomerId { get; set; }

    public virtual Client Customer { get; set; } = null!;

    public virtual CustomersGroup Group { get; set; } = null!;
}
