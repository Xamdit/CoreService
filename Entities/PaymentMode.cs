namespace Service.Entities;

public partial class PaymentMode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int ShowOnPdf { get; set; }

    public int InvoicesOnly { get; set; }

    public int ExpensesOnly { get; set; }

    public int SelectedByDefault { get; set; }

    public bool Active { get; set; }
}
