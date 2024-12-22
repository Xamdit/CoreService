namespace Service.Models.Invoices;

public class InvoicePostData
{
  public int? Currency { get; set; }
  public int? CustomerId { get; set; }
  public int? ProjectId { get; set; }
  public List<int> Years { get; set; }
}
