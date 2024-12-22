namespace Service.Models.CreditNotes;

public class CreditNoteRefundStatement
{
  public int CreditNoteRefundId { get; set; }
  public int CreditNoteId { get; set; }
  public int Amount { get; set; }
  public string TmpDate { get; set; }
  public string Date { get; set; }
  public int PaymentId { get; set; }
  public int? PaymentInvoiceId { get; set; }
  public int CreditId { get; set; }
  public int InvoiceId { get; set; }
  public int CreditAppliedCreditNoteId { get; set; }
  public double Total { get; set; }
  public string? DueDate { get; set; }
}
