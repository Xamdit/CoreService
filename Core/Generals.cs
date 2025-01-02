using Microsoft.AspNetCore.Components;
using Service.Entities;
using File = System.IO.File;

namespace Service.Core;

public class Generals
{
  public static string json(string filename)
  {
    return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), filename));
  }

  public static MarkupString markup(string str)
  {
    return new MarkupString(str);
  }

  public static void log_activity(string message)
  {
  }

  public static void log_activity(int id, params object[] message)
  {
  }

  public static void set_alert(string title, string message)
  {
  }

  public static int total_left_to_pay(CreditNote creditNote)
  {
    return 0;
  }
}
