namespace Service.Entities;

public partial class ConsentPurpose
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime DateCreated { get; set; }

    public string? LastUpdated { get; set; }

    public virtual ICollection<Consent> Consents { get; set; } = new List<Consent>();
}
