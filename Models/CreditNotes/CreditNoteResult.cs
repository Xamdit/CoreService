using Service.Entities;
using File = Service.Entities.File;

namespace Service.Models.CreditNotes;

public class CreditNoteResult
{
  public int total_refunds { get; set; }
  public List<Itemable> items { get; set; }
  public int credits_used { get; set; }
  public Invoice Invoice { get; set; }
  public double TotalLeftToPay { get; set; }
  public CreditNote CreditNote { get; set; }
  public List<CreditNoteRefund> CreditNoteRefunds { get; set; }
  public Entities.Client Client { get; set; }
  public string remaining_credits { get; set; }
  public List<Credit> applied_credits { get; set; }
  public List<File> attachments { get; set; }
}
