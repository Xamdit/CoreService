using Service.Entities;

namespace Service.Models.Extras;

public static class PaymentModeExtension
{
  public static List<Option> GetSettings(this PaymentMode self)
  {
    var output = new List<Option>();
    return output;
  }
}
