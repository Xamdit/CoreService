namespace Service.Entities;

public partial class UserAutoLogin
{
    public int Id { get; set; }

    public string? Key { get; set; }

    public int? UserId { get; set; }

    public string UserAgent { get; set; } = null!;

    public string LastIp { get; set; } = null!;

    public string LastLogin { get; set; } = null!;

    public bool IsStaff { get; set; }
}
