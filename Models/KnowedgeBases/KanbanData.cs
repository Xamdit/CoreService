namespace Service.Models.KnowedgeBases;

public class KanbanData
{
  public List<List<int>> Order { get; set; } = new();
  public int? GroupId { get; set; }
}
