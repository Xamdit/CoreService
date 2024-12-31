namespace Service.Entities;

public partial class Expense
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public int Currency { get; set; }

    public int Amount { get; set; }

    public int? Tax { get; set; }

    public int Tax2 { get; set; }

    public string ReferenceNo { get; set; } = null!;

    public string? Note { get; set; }

    public string ExpenseName { get; set; } = null!;

    public int? ClientId { get; set; }

    public int? ProjectId { get; set; }

    public bool? Billable { get; set; }

    public int? InvoiceId { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string Date { get; set; } = null!;

    public string? RecurringType { get; set; }

    public string? RepeatEvery { get; set; }

    public bool Recurring { get; set; }

    public int Cycles { get; set; }

    public int TotalCycles { get; set; }

    public bool CustomRecurring { get; set; }

    public string? LastRecurringDate { get; set; }

    public bool? CreateInvoiceBillable { get; set; }

    public bool SendInvoiceToCustomer { get; set; }

    public int? RecurringFrom { get; set; }

    public DateTime DateCreated { get; set; }

    public int AddedFrom { get; set; }

    public virtual ExpensesCategory? Category { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Project? Project { get; set; }
}
