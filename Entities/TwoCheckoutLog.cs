namespace Service.Entities;

public partial class TwoCheckoutLog
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public string Reference { get; set; } = null!;

    public int InvoiceId { get; set; }

    public string Amount { get; set; } = null!;

    public string AttemptReference { get; set; } = null!;

    public virtual Invoice Invoice { get; set; } = null!;
}
