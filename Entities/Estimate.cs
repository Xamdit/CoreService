namespace Service.Entities;

public partial class Estimate
{
    public int Id { get; set; }

    public int Sent { get; set; }

    public string? DateSend { get; set; }

    public int? ClientId { get; set; }

    public string DeletedCustomerName { get; set; } = null!;

    public int? ProjectId { get; set; }

    public int Number { get; set; }

    public string Prefix { get; set; } = null!;

    public int NumberFormat { get; set; }

    public string Hash { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public DateTime Date { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int CurrencyId { get; set; }

    public bool Subtotal { get; set; }

    public bool TotalTax { get; set; }

    public double Total { get; set; }

    public bool Adjustment { get; set; }

    public int AddedFrom { get; set; }

    public int Status { get; set; }

    public string? ClientNote { get; set; }

    public string? AdminNote { get; set; }

    public double DiscountPercent { get; set; }

    public double DiscountTotal { get; set; }

    public string DiscountType { get; set; } = null!;

    public int? InvoiceId { get; set; }

    public string? InvoicedDate { get; set; }

    public string? Terms { get; set; }

    public string ReferenceNo { get; set; } = null!;

    public int SaleAgent { get; set; }

    public string BillingStreet { get; set; } = null!;

    public string BillingCity { get; set; } = null!;

    public string BillingState { get; set; } = null!;

    public string BillingZip { get; set; } = null!;

    public int? BillingCountry { get; set; }

    public string ShippingStreet { get; set; } = null!;

    public string ShippingCity { get; set; } = null!;

    public string ShippingState { get; set; } = null!;

    public string ShippingZip { get; set; } = null!;

    public int? ShippingCountry { get; set; }

    public bool IncludeShipping { get; set; }

    public bool ShowShippingOnEstimate { get; set; }

    public int ShowQuantityAs { get; set; }

    public int? PipelineOrder { get; set; }

    public bool IsExpiryNotified { get; set; }

    public string AcceptanceFirstName { get; set; } = null!;

    public string AcceptanceLastName { get; set; } = null!;

    public string AcceptanceEmail { get; set; } = null!;

    public string? AcceptanceDate { get; set; }

    public string AcceptanceIp { get; set; } = null!;

    public string Signature { get; set; } = null!;

    public string ShortLink { get; set; } = null!;

    public virtual Client? Client { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual Invoice? Invoice { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual Staff SaleAgentNavigation { get; set; } = null!;
}
