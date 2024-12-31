namespace Service.Entities;

public partial class SpamFilter
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public string RelType { get; set; } = null!;

    public string Value { get; set; } = null!;
}
