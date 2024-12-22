using Global.Entities;
using Service.Models.Estimates;
using Task = Global.Entities.Task;

namespace Service.Models.Invoices;

public class InvoiceOption : Invoice
{
  public List<Task> billed_tasks = new();
  public List<Expense> billed_expenses = new();
  public List<Taggable> tags = new();
  public bool save_as_draft = false;
  public bool save_and_send_later = false;
  public string repeat_every_custom = string.Empty;
  public string repeat_type_custom = string.Empty;
  public List<CustomField> custom_fields { get; set; } = new();
  public List<int> invoices_to_merge { get; set; } = new();
  public List<ItemableOption> newitems { get; set; } = new();
}
