namespace Service.Models.Gdpr;

public class ConsentDto
{
  public int Id { get; set; }
  public string PurposeName { get; set; }
  public DateTime Date { get; set; }
  public string Action { get; set; }
  public string Ip { get; set; }
}
