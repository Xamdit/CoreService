using Service.Entities;
using Service.Models.Estimates;

namespace Service.Models.CreditNotes;

public class CreditNoteDto : CreditNote
{
  public CreditNoteOption option { get; set; } = new();
  public CreditNote creditNote { get; set; } = new();
  public List<Itemable> newitems { get; set; } = new();
  public List<CustomField> custom_fields { get; set; } = new();
  public List<Itemable> remove_items { get; set; } = new();
  public DateTime Date { get; set; }
  public List<ItemableOption> NewItems { get; set; } = new();
}
