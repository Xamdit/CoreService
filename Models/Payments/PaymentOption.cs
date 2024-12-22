using Global.Entities;

namespace Service.Models.Payments;

public class PaymentOption
{
  public bool DoNotRedirect { get; set; } = false;
  public PaymentAttempt PaymentAttempt { get; set; } = new();
  public bool DoNotSendEmailTemplate { get; set; } = false;
}
