namespace Service.Entities;

public partial class ColumnInfo
{
    public int Id { get; set; }

    public string Field { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Null { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Default { get; set; } = null!;

    public string Extra { get; set; } = null!;
}
