namespace Service.Framework.Helpers.Entities;

public class ChartDataset
{
  public string Label { get; set; } = string.Empty;
  public string BackgroundColor { get; set; } = "rgba(0, 0, 0, 0.1)";
  public string BorderColor { get; set; } = "rgba(0, 0, 0, 1)";
  public int BorderWidth { get; set; } = 1;
  public bool Tension { get; set; } = false;
  public List<int> Data { get; set; } = new();
  public Dictionary<string, object> AdditionalOptions { get; set; } = new();
  public List<string> HoverBackgroundColor { get; set; } = new();
}

public class Chart
{
  public List<string> Labels { get; set; } = new();
  public List<ChartDataset> Datasets { get; set; } = new();
}
