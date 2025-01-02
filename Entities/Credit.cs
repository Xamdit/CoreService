namespace Service.Entities;

public partial class Credit
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public int CreditId { get; set; }

    public int StaffId { get; set; }

    public DateTime Date { get; set; }

    public DateTime DateApplied { get; set; }

    public int Amount { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
