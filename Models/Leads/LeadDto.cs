using Global.Entities;

namespace Service.Models.Leads;

public class LeadDto
{
  public Lead lead { get; set; } = new();
  public LeadOption option { get; set; } = new();
  public List<Taggable> Tags { get; set; } = new();
  public List<CustomField> CustomFields { get; set; } = new();
  public bool contacted_today { get; set; } = false;
  public object? custom_contact_date { get; set; }
  public List<Global.Entities.File> remove_attachments { get; set; } = new();
}
