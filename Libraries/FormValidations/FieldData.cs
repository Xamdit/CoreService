namespace Service.Libraries.FormValidations;

public class FieldData
{
  public string Field { get; set; }
  public string Label { get; set; }
  public List<string> Rules { get; set; }
  public Dictionary<string, string> Errors { get; set; }
  public bool IsArray { get; set; }
  public List<string> Keys { get; set; }
  public string PostData { get; set; }
  public string Error { get; set; }
}
