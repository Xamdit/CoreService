namespace Service.Entities;

public partial class Currency
{
  public int Id { get; set; }

  public string Symbol { get; set; } = null!;

  public string Name { get; set; } = null!;

  public string DecimalSeparator { get; set; } = null!;

  public string ThousandSeparator { get; set; } = null!;

  public string Placement { get; set; } = null!;

  public bool IsDefault { get; set; }

  public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

  public virtual ICollection<Estimate> Estimates { get; set; } = new List<Estimate>();

  public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
