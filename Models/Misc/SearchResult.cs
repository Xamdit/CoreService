namespace Service.Models.Misc;

public class SearchResult<T>
{
  public List<T> Result { get; set; } = new();
  public string Type { get; set; } = typeof(T).Name.ToLower();
  public string SearchHeading { get; set; } = typeof(T).Name.ToLower();
}
