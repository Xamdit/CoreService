namespace Service.Models.Contracts;

public class MailAttachment
{
  public string filename { get; set; }
  public bool attachment { get; set; }
  public string type { get; set; }
}
