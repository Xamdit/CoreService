namespace Service.Entities;

public partial class Invoice
{
    public int Id { get; set; }

    public int Sent { get; set; }

    public DateTime? DateSend { get; set; }

    public int ClientId { get; set; }

    public string DeletedCustomerName { get; set; } = null!;

    public int Number { get; set; }

    public string Prefix { get; set; } = null!;

    public int NumberFormat { get; set; }

    public DateTime DateCreated { get; set; }

    public string Date { get; set; } = null!;

    public string? DueDate { get; set; }

    public int? CurrencyId { get; set; }

    public bool Subtotal { get; set; }

    public double TotalTax { get; set; }

    public double Total { get; set; }

    public bool Adjustment { get; set; }

    public int? AddedFrom { get; set; }

    public string Hash { get; set; } = null!;

    public int? Status { get; set; }

    public string? ClientNote { get; set; }

    public string? AdminNote { get; set; }

    public string? LastOverdueReminder { get; set; }

    public string? LastDueReminder { get; set; }

    public int CancelOverdueReminders { get; set; }

    public string? AllowedPaymentModes { get; set; }

    public string? Token { get; set; }

    public double DiscountPercent { get; set; }

    public double DiscountTotal { get; set; }

    public string DiscountType { get; set; } = null!;

    public string Recurring { get; set; } = null!;

    public string RecurringType { get; set; } = null!;

    public int CustomRecurring { get; set; }

    public int Cycles { get; set; }

    public int TotalCycles { get; set; }

    public int? IsRecurringFrom { get; set; }

    public string? LastRecurringDate { get; set; }

    public string? Terms { get; set; }

    public int? SaleAgent { get; set; }

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

    public bool ShowShippingOnInvoice { get; set; }

    public int ShowQuantityAs { get; set; }

    public int? ProjectId { get; set; }

    public int SubscriptionId { get; set; }

    public string ShortLink { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();

    public virtual Currency? Currency { get; set; }

    public virtual ICollection<Estimate> Estimates { get; set; } = new List<Estimate>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<InvoicePaymentRecord> InvoicePaymentRecords { get; set; } = new List<InvoicePaymentRecord>();

    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();

    public virtual Project? Project { get; set; }

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual Staff? SaleAgentNavigation { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<TwoCheckoutLog> TwoCheckoutLogs { get; set; } = new List<TwoCheckoutLog>();
}
