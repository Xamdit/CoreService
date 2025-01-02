namespace Service.Entities;

public partial class Template
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int AddedFrom { get; set; }

    public string? Content { get; set; }
}
