namespace Service.Entities;

public partial class Session
{
    public int Id { get; set; }

    public string IpAddress { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public byte[] Data { get; set; } = null!;
}
