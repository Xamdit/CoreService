namespace Service.Models.Leads;

public class LeadOption
{
  public List<int> NotifyIdsStaff { get; set; } = new();
  public List<int> NotifyIdsRoles { get; set; } = new();
  public object custom_contact_date { get; set; }
  public bool contacted_today { get; set; }
}
