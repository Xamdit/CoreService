namespace Service.Helpers.Sale;

public class TaxInfo
{
  public string TaxName { get; set; }
  public decimal TaxRate { get; set; }
  public List<double> Totals { get; set; } = new();
}
