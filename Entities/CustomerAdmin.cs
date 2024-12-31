namespace Service.Entities;

public partial class CustomerAdmin
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int CustomerId { get; set; }

    public string DateAssigned { get; set; } = null!;

    public virtual Client Customer { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
