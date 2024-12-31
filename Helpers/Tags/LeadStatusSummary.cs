using Service.Entities;

namespace Service.Helpers.Tags;

public class LeadStatusSummary : LeadsStatus
{
  public bool Lost { get; set; }
  public string Color { get; set; }
  public double Percent { get; set; }
  public int Total { get; set; }
  public decimal Value { get; set; }
}
