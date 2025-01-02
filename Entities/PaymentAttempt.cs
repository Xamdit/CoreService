namespace Service.Entities;

public partial class PaymentAttempt
{
    public int Id { get; set; }

    public string Reference { get; set; } = null!;

    public int? InvoiceId { get; set; }

    public int Amount { get; set; }

    public int Fee { get; set; }

    public string PaymentGateway { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Invoice? Invoice { get; set; }
}
