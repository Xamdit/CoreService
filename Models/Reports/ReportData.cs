namespace Service.Models.Reports;

public class ReportData
{
  public Dictionary<string, string> Months { get; set; }
  public Dictionary<string, List<int>> Temp { get; set; }
  public List<int> Total { get; set; }
  public List<string> Labels { get; set; }
}
