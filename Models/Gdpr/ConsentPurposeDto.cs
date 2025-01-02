using Service.Entities;

namespace Service.Models.Gdpr;

public class ConsentPurposeDto : ConsentPurpose
{
  public string Name { get; set; }
  public int TotalUsage { get; set; }
  public bool ConsentGiven { get; set; }
  public bool LastActionIsOptOut { get; set; }
  public DateTime? ConsentLastUpdated { get; set; }
}
