namespace Service.Entities;

public partial class CreditNoteRefund
{
    public int Id { get; set; }

    public int CreditNoteId { get; set; }

    public int StaffId { get; set; }

    public string RefundedOn { get; set; } = null!;

    public string PaymentMode { get; set; } = null!;

    public string? Note { get; set; }

    public int Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CreditNote CreditNote { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
