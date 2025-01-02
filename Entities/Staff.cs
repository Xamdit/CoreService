namespace Service.Entities;

public partial class Staff
{
  public int Id { get; set; }

  public string Email { get; set; } = null!;

  public string FirstName { get; set; } = null!;

  public string LastName { get; set; } = null!;

  public string? Facebook { get; set; }

  public string? LinkedIn { get; set; }

  public string PhoneNumber { get; set; } = null!;

  public string Skype { get; set; } = null!;

  public string Password { get; set; } = null!;

  public DateTime DateCreated { get; set; }

  public string ProfileImage { get; set; } = null!;

  public string LastIp { get; set; } = null!;

  public DateTime? LastLogin { get; set; }

  public string? LastActivity { get; set; }

  public DateTime? LastPasswordChange { get; set; }

  public string? NewPassKey { get; set; }

  public string? NewPassKeyRequested { get; set; }

  public bool IsAdmin { get; set; }

  public int? Role { get; set; }

  public bool? Active { get; set; }

  public string DefaultLanguage { get; set; } = null!;

  public string MediaPathSlug { get; set; } = null!;

  public int IsNotStaff { get; set; }

  public bool HourlyRate { get; set; }

  public int TwoFactorAuthEnabled { get; set; }

  public string TwoFactorAuthCode { get; set; } = null!;

  public DateTime? TwoFactorAuthCodeRequested { get; set; }

  public string? EmailSignature { get; set; }

  public string? GoogleAuthSecret { get; set; }

  public string Uuid { get; set; } = null!;

  public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

  public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

  public virtual ICollection<ContractComment> ContractComments { get; set; } = new List<ContractComment>();

  public virtual ICollection<ContractRenewal> ContractRenewals { get; set; } = new List<ContractRenewal>();

  public virtual ICollection<CreditNoteRefund> CreditNoteRefunds { get; set; } = new List<CreditNoteRefund>();

  public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();

  public virtual ICollection<CustomerAdmin> CustomerAdmins { get; set; } = new List<CustomerAdmin>();

  public virtual ICollection<Estimate> Estimates { get; set; } = new List<Estimate>();

  public virtual ICollection<Event> Events { get; set; } = new List<Event>();

  public virtual ICollection<File> Files { get; set; } = new List<File>();

  public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

  public virtual ICollection<PinnedProject> PinnedProjects { get; set; } = new List<PinnedProject>();

  public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();

  public virtual ICollection<ProjectDiscussionComment> ProjectDiscussionComments { get; set; } = new List<ProjectDiscussionComment>();

  public virtual ICollection<ProjectDiscussion> ProjectDiscussions { get; set; } = new List<ProjectDiscussion>();

  public virtual ICollection<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();

  public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

  public virtual ICollection<ProjectNote> ProjectNotes { get; set; } = new List<ProjectNote>();

  public virtual ICollection<ProposalComment> ProposalComments { get; set; } = new List<ProposalComment>();

  public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

  public virtual ICollection<SalesActivity> SalesActivities { get; set; } = new List<SalesActivity>();

  public virtual ICollection<StaffDepartment> StaffDepartments { get; set; } = new List<StaffDepartment>();

  public virtual ICollection<StaffPermission> StaffPermissions { get; set; } = new List<StaffPermission>();

  public virtual ICollection<TaskAssigned> TaskAssigneds { get; set; } = new List<TaskAssigned>();

  public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

  public virtual ICollection<TaskFollower> TaskFollowers { get; set; } = new List<TaskFollower>();

  public virtual ICollection<TasksTimer> TasksTimers { get; set; } = new List<TasksTimer>();

  public virtual ICollection<TicketReply> TicketReplies { get; set; } = new List<TicketReply>();

  public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

  public virtual ICollection<Todo> Todos { get; set; } = new List<Todo>();

  public virtual ICollection<UserMetum> UserMeta { get; set; } = new List<UserMetum>();
}
