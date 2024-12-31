namespace Service.Entities;

public partial class ViewsTracking
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public DateTime Date { get; set; }

    public string ViewIp { get; set; } = null!;
}
