namespace Service.Entities;

public partial class Todo
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int StaffId { get; set; }

    public DateTime DateCreated { get; set; }

    public int Finished { get; set; }

    public string? DateFinished { get; set; }

    public int? ItemOrder { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
