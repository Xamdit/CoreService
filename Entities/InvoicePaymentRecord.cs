namespace Service.Entities;

public partial class InvoicePaymentRecord
{
    public int Id { get; set; }

    public int? InvoiceId { get; set; }

    public string Name { get; set; } = null!;

    public double Amount { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string Date { get; set; } = null!;

    public string DateRecorded { get; set; } = null!;

    public string? Note { get; set; }

    public string? TransactionId { get; set; }

    public virtual Invoice? Invoice { get; set; }
}
