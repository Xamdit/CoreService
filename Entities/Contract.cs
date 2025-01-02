namespace Service.Entities;

public partial class Contract
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public string? Description { get; set; }

    public string Subject { get; set; } = null!;

    public int Client { get; set; }

    public DateTime? DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public bool? ContractType { get; set; }

    public int? ProjectId { get; set; }

    public int AddedFrom { get; set; }

    public DateTime DateCreated { get; set; }

    public int IsExpiryNotified { get; set; }

    public int ContractValue { get; set; }

    public bool Trash { get; set; }

    public string NotVisibleToClient { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public bool Signed { get; set; }

    public string Signature { get; set; } = null!;

    public int MarkedAsSigned { get; set; }

    public string AcceptanceFirstName { get; set; } = null!;

    public string AcceptanceLastName { get; set; } = null!;

    public bool AcceptanceEmail { get; set; }

    public string? AcceptanceDate { get; set; }

    public string AcceptanceIp { get; set; } = null!;

    public string ShortLink { get; set; } = null!;

    public DateTime? LastSentAt { get; set; }

    public string? ContactsSentTo { get; set; }

    public string? LastSignReminderAt { get; set; }

    public virtual ICollection<ContractComment> ContractComments { get; set; } = new List<ContractComment>();

    public virtual ICollection<ContractRenewal> ContractRenewals { get; set; } = new List<ContractRenewal>();

    public virtual Project? Project { get; set; }
}
