namespace Service.Libraries;

public class Email
{
  public Email clear(bool b)
  {
    return this;
  }

  public Email set_newline(object configItem)
  {
    return this;
  }

  public Email from(object o, string cnfFromName)
  {
    return this;
  }

  public Email to(object o)
  {
    return this;
  }

  public Email bcc(string s)
  {
    return this;
  }

  public Email cc(object o)
  {
    return this;
  }

  public bool send()
  {
    return false;
  }
}
