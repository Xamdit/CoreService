using Service.Entities;

namespace Service.Models.Statements;

public class StatementResult
{
  public object MergedResults { get; set; }
  public double InvoicedAmount { get; set; }
  public double CreditNotesAmount { get; set; }
  public double RefundsAmount { get; set; }
  public double AmountPaid { get; set; }
  public double BeginningBalance { get; set; }
  public double BalanceDue { get; set; }
  public int ClientId { get; set; }
  public Entities.Client Client { get; set; }
  public DateTime From { get; set; }
  public DateTime To { get; set; }
  public Currency Currency { get; set; }
}
