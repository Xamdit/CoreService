using Global.Entities;

namespace Service.Models.Estimates;

public class CustomFieldOption
{
  // public List<CustomField> Items = new();
  public List<string> Items = new();
}

public class ItemableOption : Itemable
{
  public List<string> TaxNames { get; set; } = new();
  public CustomFieldOption CustomFields { get; } = new();
  public List<string> Names { get; set; }

  public int Order = 0;
}
