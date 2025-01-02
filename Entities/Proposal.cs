namespace Service.Entities;

public partial class Proposal
{
    public int Id { get; set; }

    public string Subject { get; set; } = null!;

    public string? Content { get; set; }

    public int AddedFrom { get; set; }

    public DateTime DateCreated { get; set; }

    public bool Total { get; set; }

    public bool Subtotal { get; set; }

    public bool TotalTax { get; set; }

    public bool Adjustment { get; set; }

    public bool DiscountPercent { get; set; }

    public bool DiscountTotal { get; set; }

    public string DiscountType { get; set; } = null!;

    public int ShowQuantityAs { get; set; }

    public int Currency { get; set; }

    public DateTime? OpenTill { get; set; }

    public DateTime Date { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public int Assigned { get; set; }

    public string Hash { get; set; } = null!;

    public string ProposalTo { get; set; } = null!;

    public int? ProjectId { get; set; }

    public int CountryId { get; set; }

    public string Zip { get; set; } = null!;

    public string State { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public int AllowComments { get; set; }

    public int Status { get; set; }

    public int? EstimateId { get; set; }

    public int? InvoiceId { get; set; }

    public DateTime? DateConverted { get; set; }

    public int? PipelineOrder { get; set; }

    public int IsExpiryNotified { get; set; }

    public string AcceptanceFirstName { get; set; } = null!;

    public string AcceptanceLastName { get; set; } = null!;

    public bool AcceptanceEmail { get; set; }

    public string? AcceptanceDate { get; set; }

    public string AcceptanceIp { get; set; } = null!;

    public string Signature { get; set; } = null!;

    public string ShortLink { get; set; } = null!;

    public virtual Country Country { get; set; } = null!;

    public virtual Estimate? Estimate { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ICollection<ProposalComment> ProposalComments { get; set; } = new List<ProposalComment>();
}
