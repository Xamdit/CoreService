namespace Service.Entities;

public partial class Module
{
    public int Id { get; set; }

    public string ModuleName { get; set; } = null!;

    public string InstalledVersion { get; set; } = null!;

    public int Active { get; set; }
}
