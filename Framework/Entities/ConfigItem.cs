namespace Service.Framework.Entities;

public class ConfigItem
{
  public int Id { get; set; }

  public string? Group { get; set; }

  public string? Name { get; set; }

  public object? Value { get; set; }
}
