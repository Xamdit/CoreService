namespace Service.Models.Invoices;

public class InvoiceStatus
{
  public const int STATUS_UNPAID = 1;

  public const int STATUS_PAID = 2;

  public const int STATUS_PARTIALLY = 3;

  public const int STATUS_OVERDUE = 4;

  public const int STATUS_CANCELLED = 5;

  public const int STATUS_DRAFT = 6;

  public const int STATUS_DRAFT_NUMBER = 1000000000;
}
