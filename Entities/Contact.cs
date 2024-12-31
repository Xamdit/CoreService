namespace Service.Entities;

public partial class Contact
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool? IsPrimary { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string Password { get; set; } = null!;

    public string NewPassKey { get; set; } = null!;

    public string? NewPassKeyRequested { get; set; }

    public string? EmailVerifiedAt { get; set; }

    public string EmailVerificationKey { get; set; } = null!;

    public string? EmailVerificationSentAt { get; set; }

    public string LastIp { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public DateTime? LastPasswordChange { get; set; }

    public bool Active { get; set; }

    public string ProfileImage { get; set; } = null!;

    public string Direction { get; set; } = null!;

    public int InvoiceEmails { get; set; }

    public int EstimateEmails { get; set; }

    public int CreditNoteEmails { get; set; }

    public int ContractEmails { get; set; }

    public int TaskEmails { get; set; }

    public int ProjectEmails { get; set; }

    public int TicketEmails { get; set; }

    public string Uuid { get; set; } = null!;

    public virtual ICollection<Consent> Consents { get; set; } = new List<Consent>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<GdprRequest> GdprRequests { get; set; } = new List<GdprRequest>();

    public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();

    public virtual ICollection<ProjectDiscussionComment> ProjectDiscussionComments { get; set; } = new List<ProjectDiscussionComment>();

    public virtual ICollection<ProjectDiscussion> ProjectDiscussions { get; set; } = new List<ProjectDiscussion>();

    public virtual ICollection<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();

    public virtual ICollection<SharedCustomerFile> SharedCustomerFiles { get; set; } = new List<SharedCustomerFile>();

    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

    public virtual ICollection<TicketReply> TicketReplies { get; set; } = new List<TicketReply>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<UserMetum> UserMeta { get; set; } = new List<UserMetum>();
}
