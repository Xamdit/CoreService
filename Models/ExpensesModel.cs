using System.Linq.Expressions;
using Service.Entities;
using Service.Framework;
using Service.Helpers;
using Service.Helpers.Sale;

namespace Service.Models;

public class ExpensesModel(MyInstance self, MyContext db) : MyModel(self)
{
  public int? add(Expense expense, List<CustomField> customFields = default, string? RepeatEveryCustom = null, string? RepeatTypeCustom = null)
  {
    expense.Date = expense.Date;
    expense.Note = ConvertToHtmlNewLine(expense.Note);

    expense.Billable = expense.Billable.HasValue ? expense.Billable.Value : false;
    expense.CreateInvoiceBillable = expense.CreateInvoiceBillable.HasValue && expense.CreateInvoiceBillable.Value;

    // expense.ProjectId = expense.ProjectId ?? 0;
    expense.AddedFrom = GetCurrentStaffId();
    expense.DateCreated = DateTime.UtcNow;

    if (!string.IsNullOrEmpty(expense.RepeatEvery))
    {
      expense.Recurring = true;
      if (expense.RepeatEvery == "custom")
      {
        expense.RepeatEvery = RepeatEveryCustom;
        expense.RecurringType = RepeatTypeCustom;
        expense.CustomRecurring = true;
      }
      else
      {
        var temp = expense.RepeatEvery.Split('-');
        expense.RecurringType = temp[1];
        expense.RepeatEvery = temp[0];
        expense.CustomRecurring = false;
      }
    }
    else
    {
      expense.Recurring = false;
    }

    db.Expenses.Add(expense);
    db.SaveChanges();

    var insertId = expense.Id;
    if (insertId <= 0) return null;
    if (customFields.Any()) self.helper.handle_custom_fields_post(insertId, customFields);

    if (expense.ProjectId > 0)
    {
      var projectSettings = db.Projects
        .Where(p => p.Id == expense.ProjectId)
        .SelectMany(p => p.ProjectSettings)
        .ToList();

      var visibleActivity = projectSettings.Any(s => s is { Name: "view_finance_overview", Value: "1" });
      var addedExpense = db.Expenses.FirstOrDefault(e => e.Id == insertId);
      var activityAdditionalData = addedExpense?.ExpenseName;

      log_project_activity(expense.ProjectId.Value, "project_activity_recorded_expense", activityAdditionalData, visibleActivity);
    }

    log_activity($"New Expense Added [{insertId}]");
    return insertId;
  }

  public List<Expense> GetChildExpenses(int id)
  {
    return db.Expenses.Where(e => e.RecurringFrom == id).ToList();
  }

  public decimal GetExpensesTotal(Expense queryData)
  {
    var currency = self.helper.get_currency(queryData.Currency) ?? self.helper.get_base_currency();
    var expenses = db.Expenses
      .Where(e => e.Currency == currency.Id)
      .ToList();

    decimal total = 0;
    foreach (var expense in expenses)
    {
      total += expense.Amount;
      total += calculate_tax(expense.Tax, expense.Amount);
      total += calculate_tax(expense.Tax2, expense.Amount);
    }

    return total;
  }

  public bool Update(Expense data, int id)
  {
    var originalExpense = db.Expenses.Find(id);

    data.Date = data.Date;
    data.Note = ConvertToHtmlNewLine(data.Note);

    if (!string.IsNullOrEmpty(originalExpense.RepeatEvery) && string.IsNullOrEmpty(data.RepeatEvery))
    {
      data.Cycles = 0;
      data.TotalCycles = 0;
      data.LastRecurringDate = null;
    }

    if (!string.IsNullOrEmpty(data.RepeatEvery))
    {
      data.Recurring = true;
      if (data.RepeatEvery == "custom")
      {
        // data.RepeatEvery = data.RepeatEveryCustom;
        // data.RecurringType = data.RepeatTypeCustom;
        data.CustomRecurring = true;
      }
      else
      {
        var temp = data.RepeatEvery.Split('-');
        data.RecurringType = temp[1];
        data.RepeatEvery = temp[0];
        data.CustomRecurring = false;
      }
    }
    else
    {
      data.Recurring = false;
    }

    db.Expenses.Update(data);
    var updated = db.SaveChanges() > 0;

    if (updated) log_activity($"Expense Updated [{id}]");

    return updated;
  }

  public bool delete(int id)
  {
    var expense = db.Expenses.Find(id);

    if (expense == null) return false;

    db.Expenses.Remove(expense);
    var deleted = db.SaveChanges() > 0;

    if (deleted) log_activity($"Expense Deleted [{id}]");

    return deleted;
  }

  private void handle_custom_field_post(int id, Dictionary<string, string> customFields)
  {
    // Handle custom fields logic
  }

  private decimal calculate_tax(decimal? taxRate, decimal amount)
  {
    if (taxRate is > 0) return amount / 100 * taxRate.Value;

    return 0;
  }


  private string ConvertToHtmlNewLine(string text)
  {
    // Convert new lines to HTML <br> tags
    return text.Replace("\n", "<br>");
  }

  private int GetCurrentStaffId()
  {
    // Get the current staff user id (dummy implementation)
    return 1;
  }


  private void log_project_activity(int projectId, string activityType, string additionalData, bool visible)
  {
    // Log project activity
    Console.WriteLine($"Project Activity: {activityType}, Data: {additionalData}, Visible: {visible}");
  }

  public List<Expense> get(Expression<Func<Expense, bool>> condition)
  {
    var rows = db.Expenses.Where(condition).ToList();
    return rows;
  }
}
