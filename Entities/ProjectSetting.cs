namespace Service.Entities;

public partial class ProjectSetting
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public virtual Project Project { get; set; } = null!;
}
