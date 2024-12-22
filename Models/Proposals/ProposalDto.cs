using Global.Entities;
using Service.Models.Estimates;

namespace Service.Models.Proposals;

public class ProposalDto : Proposal
{
  public List<ItemableOption> newitems = new();
  public List<Taggable> Tags { get; set; } = new();
  public List<CustomField> custom_fields { get; set; } = new();
  public bool save_and_send { get; set; }
  public int estimate_request_id { get; set; } = 0;
  public List<int> removed_items { get; set; } = new();
}
