namespace Service.Models.Invoices;

public class InvoiceTotalsResult
{
  public List<double> Due { get; set; } = new();
  public List<double> Paid { get; set; } = new();
  public List<double> Overdue { get; set; } = new();
  public double DueTotal { get; set; }
  public double PaidTotal { get; set; }
  public double OverdueTotal { get; set; }
  public string Currency { get; set; }
  public int CurrencyId { get; set; }
}
