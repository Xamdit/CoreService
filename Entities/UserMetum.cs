namespace Service.Entities;

public partial class UserMetum
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int ClientId { get; set; }

    public int ContactId { get; set; }

    public string MetaKey { get; set; } = null!;

    public string? MetaValue { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
