namespace Service.Framework.Schemas;

public class SuccessResponse<T>
{
  public bool Success { get; set; }
  public string Type { get; set; }
  public T Data { get; set; }
  public string Token { get; set; }
}
