namespace Service.Entities;

public partial class Lead
{
    public int Id { get; set; }

    public string Hash { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Company { get; set; } = null!;

    public string? Description { get; set; }

    public int? Country { get; set; }

    public string Zip { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int Assigned { get; set; }

    public DateTime DateCreated { get; set; }

    public int FromFormId { get; set; }

    public int? StatusId { get; set; }

    public int? SourceId { get; set; }

    public string? LastContact { get; set; }

    public DateTime? DateAssigned { get; set; }

    public string? LastStatusChange { get; set; }

    public int AddedFrom { get; set; }

    public string Email { get; set; } = null!;

    public string Website { get; set; } = null!;

    public int? LeadOrder { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? DateConverted { get; set; }

    public bool Lost { get; set; }

    public bool Junk { get; set; }

    public int LastLeadStatus { get; set; }

    public int IsImportedFromEmailIntegration { get; set; }

    public string EmailIntegrationUid { get; set; } = null!;

    public bool IsPublic { get; set; }

    public string DefaultLanguage { get; set; } = null!;

    public int ClientId { get; set; }

    public int LeadValue { get; set; }

    public DateTime? DateAdded { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<Consent> Consents { get; set; } = new List<Consent>();

    public virtual ICollection<GdprRequest> GdprRequests { get; set; } = new List<GdprRequest>();

    public virtual ICollection<LeadActivityLog> LeadActivityLogs { get; set; } = new List<LeadActivityLog>();

    public virtual ICollection<LeadIntegrationEmail> LeadIntegrationEmails { get; set; } = new List<LeadIntegrationEmail>();

    public virtual LeadsSource? Source { get; set; }

    public virtual LeadsStatus? Status { get; set; }
}
