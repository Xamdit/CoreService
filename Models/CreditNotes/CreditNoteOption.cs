namespace Service.Models.CreditNotes;

public class CreditNoteOption
{
  public int id { get; internal set; }
  public string color { get; internal set; }
  public string name { get; internal set; }
  public int order { get; internal set; }
  public bool filter_default { get; internal set; }
  public bool save_and_send { get; set; } = false;
}
