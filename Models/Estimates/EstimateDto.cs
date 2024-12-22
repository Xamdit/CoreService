using Global.Entities;

namespace Service.Models.Estimates;

public class EstimateDto : Estimate
{
  public bool? saveAndSend = null;
  public List<ItemableOption> newitems = new();
  public int EstimateRequestId { get; set; }
  public List<CustomField> CustomFields { get; set; } = new();
  public List<Taggable> Tags { get; set; } = new();
  public List<ItemableOption> RemovedItems { get; set; } = new();
}
