namespace Service.Entities;

public partial class Subscription
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DescriptionInItem { get; set; }

    public int? ClientId { get; set; }

    public DateTime? Date { get; set; }

    public string? Terms { get; set; }

    public int Currency { get; set; }

    public int TaxId { get; set; }

    public string StripeTaxId { get; set; } = null!;

    public int TaxId2 { get; set; }

    public string StripeTaxId2 { get; set; } = null!;

    public string? StripePlanId { get; set; }

    public string StripeSubscriptionId { get; set; } = null!;

    public int? NextBillingCycle { get; set; }

    public DateTime? EndsAt { get; set; }

    public string Status { get; set; } = null!;

    public int Quantity { get; set; }

    public int? ProjectId { get; set; }

    public string Hash { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public int CreatedFrom { get; set; }

    public DateTime? DateSubscribed { get; set; }

    public int? InTestEnvironment { get; set; }

    public DateTime? LastSentAt { get; set; }

    public virtual Client? Client { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Project? Project { get; set; }

    public virtual Taxis Tax { get; set; } = null!;
}
