namespace Service.Controllers.Admin.Authentication;

public class AuthSchema
{
  public string email { get; set; }
  public string password { get; set; }
  public bool remember { get; set; }
}

public class SetPasswordSchema
{
  public bool is_staff { get; set; }
  public int user_id { get; set; }
  public string new_pass_key { get; set; }
  public string passwordr { get; set; }
}
