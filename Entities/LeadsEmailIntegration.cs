namespace Service.Entities;

public partial class LeadsEmailIntegration
{
    public int Id { get; set; }

    public int Active { get; set; }

    public string Email { get; set; } = null!;

    public string ImapServer { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int CheckEvery { get; set; }

    public int Responsible { get; set; }

    public int LeadSource { get; set; }

    public int LeadStatus { get; set; }

    public string Encryption { get; set; } = null!;

    public string Folder { get; set; } = null!;

    public string LastRun { get; set; } = null!;

    public int NotifyLeadImported { get; set; }

    public int NotifyLeadContactMoreTimes { get; set; }

    public string NotifyType { get; set; } = null!;

    public string? NotifyIds { get; set; }

    public int MarkPublic { get; set; }

    public int OnlyLoopOnUnseenEmails { get; set; }

    public int DeleteAfterImport { get; set; }

    public int CreateTaskIfCustomer { get; set; }
}
