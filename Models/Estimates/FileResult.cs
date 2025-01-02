using File = Service.Entities.File;

namespace Service.Models.Estimates;

public class FileResult
{
  public File attachment { get; set; }
  public bool visible_attachments_to_customer_found { get; set; }
}
