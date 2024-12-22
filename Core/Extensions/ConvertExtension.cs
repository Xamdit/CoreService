using Global.Entities;
using Newtonsoft.Json;

namespace Service.Core.Extensions;

public class ConvertExtension
{
  public static T? convert<T>(object original)
  {
    var jsonString = JsonConvert.SerializeObject(original);
    return JsonConvert.DeserializeObject<T>(jsonString);
  }

  public static List<T>? converts<T>(object original)
  {
    var jsonString = JsonConvert.SerializeObject(original);
    return JsonConvert.DeserializeObject<List<T>>(jsonString);
  }

  public static Invoice? invoice(object original)
  {
    return convert<Invoice>(original);
  }

  public static Estimate estimate(object original)
  {
    return convert<Estimate>(original);
  }

  public static CreditNote credit_note(object original)
  {
    return convert<CreditNote>(original);
  }

  public static Itemable itemable(object original)
  {
    return convert<Itemable>(original);
  }

  public static Project project(object original)
  {
    return convert<Project>(original);
  }

  public static int number(object original)
  {
    return Convert.ToInt32(original);
  }
}
