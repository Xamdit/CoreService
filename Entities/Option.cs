namespace Service.Entities;

public partial class Option
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;

    public bool Autoload { get; set; }

    public string Group { get; set; } = null!;
}
