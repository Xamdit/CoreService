namespace Service.Entities;

public class ProposalsPipeline(int status)
{
  public ProposalsPipeline Search(string search)
  {
    return this;
  }

  public ProposalsPipeline Page(int page)
  {
    return this;
  }

  public ProposalsPipeline SortBy(string? sort, string? sort_by)
  {
    return this;
  }

  public ProposalsPipeline Build()
  {
    return this;
  }
}
