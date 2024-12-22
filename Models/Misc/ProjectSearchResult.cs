using Global.Entities;

namespace Service.Models.Misc;

public class ProjectSearchResult
{
  public List<Project> Result { get; set; } = new();
  public string Type { get; set; } = "expenses";
  public string SearchHeading { get; set; } = "Expenses";
}
