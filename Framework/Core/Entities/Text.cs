namespace Service.Framework.Core.Entities;

public partial class Text
{
  public int Id { get; set; }

  public string? Locale { get; set; }

  public string? Key { get; set; }

  public string? Value { get; set; }

  public int? Used { get; set; }
}
