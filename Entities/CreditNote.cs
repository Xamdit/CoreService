namespace Service.Entities;

public partial class CreditNote
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string DeletedCustomerName { get; set; } = null!;

    public int Number { get; set; }

    public string Prefix { get; set; } = null!;

    public int NumberFormat { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime Date { get; set; }

    public string? AdminNote { get; set; }

    public string? Terms { get; set; }

    public string? ClientNote { get; set; }

    public int? CurrencyId { get; set; }

    public bool Subtotal { get; set; }

    public bool TotalTax { get; set; }

    public double Total { get; set; }

    public bool Adjustment { get; set; }

    public int? AddedFrom { get; set; }

    public int? Status { get; set; }

    public int? ProjectId { get; set; }

    public double DiscountPercent { get; set; }

    public double DiscountTotal { get; set; }

    public string DiscountType { get; set; } = null!;

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

    public bool? IncludeShipping { get; set; }

    public bool? ShowShippingOnCreditNote { get; set; }

    public int ShowQuantityAs { get; set; }

    public string ReferenceNo { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<CreditNoteRefund> CreditNoteRefunds { get; set; } = new List<CreditNoteRefund>();

    public virtual Currency? Currency { get; set; }

    public virtual Project? Project { get; set; }
}
