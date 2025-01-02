namespace Service.Entities;

public partial class Client
{
    public int Id { get; set; }

    public string Company { get; set; } = null!;

    public string Vat { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public int? CountryId { get; set; }

    public string City { get; set; } = null!;

    public string Zip { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Website { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public bool Active { get; set; }

    public int? LeadId { get; set; }

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

    public string Longitude { get; set; } = null!;

    public string Latitude { get; set; } = null!;

    public string DefaultLanguage { get; set; } = null!;

    public int DefaultCurrency { get; set; }

    public int ShowPrimaryContact { get; set; }

    public string StripeId { get; set; } = null!;

    public int RegistrationConfirmed { get; set; }

    public int AddedFrom { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

    public virtual ICollection<CustomerAdmin> CustomerAdmins { get; set; } = new List<CustomerAdmin>();

    public virtual ICollection<CustomerGroup> CustomerGroups { get; set; } = new List<CustomerGroup>();

    public virtual ICollection<Estimate> Estimates { get; set; } = new List<Estimate>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<GdprRequest> GdprRequests { get; set; } = new List<GdprRequest>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Lead? Lead { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
