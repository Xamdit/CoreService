namespace Service.Entities;

public partial class Vault
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string ServerAddress { get; set; } = null!;

    public int? Port { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Description { get; set; }

    public int Creator { get; set; }

    public string CreatorName { get; set; } = null!;

    public int Visibility { get; set; }

    public bool ShareInProjects { get; set; }

    public string? LastUpdated { get; set; }

    public string LastUpdatedFrom { get; set; } = null!;

    public DateTime DateCreated { get; set; }
}
