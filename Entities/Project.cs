namespace Service.Entities;

public partial class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Status { get; set; }

    public int ClientId { get; set; }

    public int BillingType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime ProjectCreated { get; set; }

    public DateTime? DateFinished { get; set; }

    public int? Progress { get; set; }

    public int ProgressFromTasks { get; set; }

    public bool ProjectCost { get; set; }

    public double ProjectRatePerHour { get; set; }

    public bool EstimatedHours { get; set; }

    public int AddedFrom { get; set; }

    public int? ContactNotification { get; set; }

    public string? NotifyContacts { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

    public virtual ICollection<Estimate> Estimates { get; set; } = new List<Estimate>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();

    public virtual ICollection<PinnedProject> PinnedProjects { get; set; } = new List<PinnedProject>();

    public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();

    public virtual ICollection<ProjectDiscussion> ProjectDiscussions { get; set; } = new List<ProjectDiscussion>();

    public virtual ICollection<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();

    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    public virtual ICollection<ProjectNote> ProjectNotes { get; set; } = new List<ProjectNote>();

    public virtual ICollection<ProjectSetting> ProjectSettings { get; set; } = new List<ProjectSetting>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
