namespace Service.Entities;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string ImapUsername { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int EmailFromHeader { get; set; }

    public string Host { get; set; } = null!;

    public string? Password { get; set; }

    public string Encryption { get; set; } = null!;

    public string Folder { get; set; } = null!;

    public int DeleteAfterImport { get; set; }

    public string? CalendarId { get; set; }

    public bool HideFromClient { get; set; }

    public virtual ICollection<StaffDepartment> StaffDepartments { get; set; } = new List<StaffDepartment>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
