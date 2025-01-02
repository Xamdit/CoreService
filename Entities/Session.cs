namespace Service.Entities;

public partial class Session
{
    public int Id { get; set; }

    public string IpAddress { get; set; } = null!;

    public string Data { get; set; } = null!;

    public string Uuid { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsSerialize { get; set; }
}
