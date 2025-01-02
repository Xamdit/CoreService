using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;

namespace Service.Models.Reports;

public class ReportsModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  public Chart get_stats_chart_data(string label, Dictionary<string, object> where, Dictionary<string, object> datasetOptions, int year)
  {
    var chart = new Chart
    {
      Labels = new List<string>(),
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = label,
          BorderWidth = 1,
          Tension = false,
          Data = new List<int>()
        }
      }
    };

    foreach (var option in datasetOptions) chart.Datasets[0].AdditionalOptions[option.Key] = option.Value;

    var categories = db.ExpensesCategories.ToList();
    chart.Labels = categories.Select(x => x.Name).ToList();
    chart.Datasets[0].Data = categories
      .Select(
        category =>
          Convert.ToInt32(
            db.Expenses
              .Count(x =>
                x.CategoryId == category.Id &&
                DateTime.Parse(x.Date).Year == year
              )
          )
      )
      .ToList();
    return chart;
  }

  public Chart get_expenses_vs_income_report(int? year)
  {
    year ??= DateTime.Now.Year;

    var chart = new Chart
    {
      Labels = new List<string>(),
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = "Income",
          BackgroundColor = "rgba(37,155,35,0.2)",
          BorderColor = "#84c529",
          BorderWidth = 1,
          Tension = false,
          Data = new List<int>()
        },
        new()
        {
          Label = "Expenses",
          BackgroundColor = "rgba(252,45,66,0.4)",
          BorderColor = "#fc2d42",
          BorderWidth = 1,
          Tension = false,
          Data = new List<int>()
        }
      }
    };

    for (var m = 1; m <= 12; m++)
    {
      chart.Labels.Add(new DateTime(year.Value, m, 1).ToString("MMMM"));

      var monthlyExpenses = db.Expenses
        .Where(e => DateTime.Parse(e.Date).Month == m && DateTime.Parse(e.Date).Year == year)
        .Select(e => e.Amount + (e.Tax != 0 ? e.Amount / 100 * e.Tax : 0) + (e.Tax2 != 0 ? e.Amount / 100 * e.Tax2 : 0))
        .Sum();

      chart.Datasets[1].Data.Add(monthlyExpenses.Value);
      var monthlyIncome = db.InvoicePaymentRecords
        .Where(p => DateTime.Parse(p.Date).Month == m && DateTime.Parse(p.Date).Year == year)
        .Sum(p => Convert.ToInt32(p.Amount));

      chart.Datasets[0].Data.Add(monthlyIncome);
    }

    return chart;
  }

  public Chart leads_this_week_report()
  {
    var monday = self.helper.get_monday_this_week();
    var sunday = monday.AddDays(6);

    var weeklyLeads = db.Leads
      .Where(l => DateTime.Parse(l.LastStatusChange).Date >= monday && DateTime.Parse(l.LastStatusChange).Date <= sunday && l.StatusId == 1 && !l.Lost)
      .ToList();

    var chart = new Chart
    {
      Labels = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Data = new List<int> { 0, 0, 0, 0, 0, 0, 0 },
          BackgroundColor = get_system_favorite_colors().First(),
          HoverBackgroundColor = get_adjusted_colors(get_system_favorite_colors(), -20)
        }
      }
    };

    weeklyLeads
      .Select(lead => DateTime.Parse(lead.LastStatusChange).DayOfWeek + 6 % 7)
      .ToList()
      .ForEach(dayIndex =>
        chart.Datasets[0].Data[(int)dayIndex]++);

    return chart;
  }

  public Chart leads_staff_report(DateTime? fromDate = null, DateTime? toDate = null)
  {
    var staffMembers = db.Staff.ToList();
    var chart = new Chart
    {
      Labels = new List<string>(),
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = "Created Leads",
          BackgroundColor = "rgba(3,169,244,0.2)",
          BorderColor = "#03a9f4",
          BorderWidth = 1,
          Tension = false,
          Data = new List<int>()
        },
        new()
        {
          Label = "Lost Leads",
          BackgroundColor = "rgba(252,45,66,0.4)",
          BorderColor = "#fc2d42",
          BorderWidth = 1,
          Tension = false,
          Data = new List<int>()
        },
        new()
        {
          Label = "Converted Leads",
          BackgroundColor = "rgba(37,155,35,0.2)",
          BorderColor = "#84c529",
          BorderWidth = 1,
          Tension = false,
          Data = new List<int>()
        }
      }
    };

    foreach (var staff in staffMembers)
    {
      chart.Labels.Add($"{staff.FirstName} {staff.LastName}");

      var totalCreatedLeads = get_total_created_leads(staff.Id, fromDate, toDate);
      var totalLostLeads = get_total_lost_leads(staff.Id, fromDate, toDate);
      var totalConvertedLeads = get_total_converted_leads(staff.Id, fromDate, toDate);

      chart.Datasets[0].Data.Add(totalCreatedLeads);
      chart.Datasets[1].Data.Add(totalLostLeads);
      chart.Datasets[2].Data.Add(totalConvertedLeads);
    }

    return chart;
  }

  public Chart leads_sources_report()
  {
    var sources = db.LeadsSources.ToList();
    var chart = new Chart
    {
      Labels = new List<string>(),
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = "Conversions by Source",
          BackgroundColor = "rgba(124,179,66,0.5)",
          BorderColor = "#7cb342",
          Data = new List<int>()
        }
      }
    };
    sources.ForEach(source =>
    {
      chart.Labels.Add(source.Name);
      var totalConversions = db.Leads
        .Count(l => l.SourceId.Value == source.Id && l.StatusId == 1 && l.Lost != null);
      chart.Datasets[0].Data.Add(totalConversions);
    });


    return chart;
  }


  private List<string> get_system_favorite_colors()
  {
    // Simulates fetching favorite colors
    return new List<string> { "#FF5733", "#33FF57", "#3357FF" }; // Example colors
  }

  private List<string> get_adjusted_colors(List<string> colors, int adjustment)
  {
    // Adjust colors brightness (placeholder)
    return colors.Select(c => c).ToList(); // Needs implementation
  }

  private int get_total_created_leads(int staffId, DateTime? fromDate, DateTime? toDate)
  {
    return db.Leads
      .Where(l => l.Assigned == staffId || l.AddedFrom == staffId)
      .Count(l => (!fromDate.HasValue || l.DateAdded >= fromDate) && (!toDate.HasValue || l.DateAdded <= toDate));
  }

  private int get_total_lost_leads(int staffId, DateTime? fromDate, DateTime? toDate)
  {
    return db.Leads
      .Where(l => (l.Assigned == staffId || l.AddedFrom == staffId) && l.Lost)
      .Count(l => !fromDate.HasValue || (DateTime.Parse($"{l.LastStatusChange}") >= fromDate && !toDate.HasValue) || DateTime.Parse(l.LastStatusChange) <= toDate);
  }

  private int get_total_converted_leads(int staffId, DateTime? fromDate, DateTime? toDate)
  {
    return db.Leads
      .Where(l => (l.Assigned == staffId || l.AddedFrom == staffId) && l.Status != null)
      .Count(l => (!fromDate.HasValue || DateTime.Parse($"{l.LastStatusChange}") >= fromDate) && (!toDate.HasValue || DateTime.Parse($"{l.LastStatusChange}") <= toDate));
  }

  public Chart total_income_report(int year, int? reportCurrency = null)
  {
    var paymentsQuery = db.InvoicePaymentRecords
      .Where(p => DateTime.Parse(p.Date).Year == year)
      .Include(p => p.Invoice)
      .AsQueryable();

    if (reportCurrency.HasValue)
      paymentsQuery = paymentsQuery.Where(p => p.Invoice.CurrencyId == reportCurrency);

    var payments = paymentsQuery
      .Select(p => new
      {
        p.Amount,
        p.Date
      })
      .ToList();

    var data = new ReportData
    {
      Months = new Dictionary<string, string>(),
      Temp = new Dictionary<string, List<int>>(),
      Total = new List<int>(),
      Labels = new List<string>()
    };

    payments.ForEach(payment =>
    {
      var monthNumber = DateTime.Parse(payment.Date).Month;
      var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNumber);
      data.Months.TryAdd(monthName, monthName);
    });


    // Sort months
    var sortedMonths = data.Months.Keys.OrderBy(m =>
    {
      var monthInfo = DateTime.ParseExact(m, "MMMM", CultureInfo.CurrentCulture);
      return monthInfo.Month;
    }).ToList();

    foreach (var month in sortedMonths)
    {
      payments
        .Where(payment =>
          month == CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Parse(payment.Date).Month))
        .ToList()
        .ForEach(payment =>
        {
          if (!data.Temp.ContainsKey(month)) data.Temp[month] = new List<int>();
          data.Temp[month].Add(Convert.ToInt32(payment.Amount));
        });

      data.Labels.Add($"{month} - {year}");

      var monthNumber = DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month;
      var refundedAmount = calculate_refunded_amount(year, monthNumber, reportCurrency);
      data.Total.Add(data.Temp[month].Sum() - refundedAmount);
    }


    // Build chart structure
    var chart = new Chart
    {
      Labels = data.Labels,
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = "Sales Type Income",
          BackgroundColor = "rgba(37,155,35,0.2)",
          BorderColor = "#84c529",
          Tension = false,
          BorderWidth = 1,
          Data = data.Total.Select(Convert.ToInt32).ToList()
        }
      }
    };

    return chart;
  }

  private int calculate_refunded_amount(int year, int month, int? currency = null)
  {
    var refundQuery = db.CreditNoteRefunds
      .Where(r => r.CreatedAt.Year == year && r.CreatedAt.Month == month);
    if (currency.HasValue)
      refundQuery = refundQuery.Where(r => r.CreditNote.Currency!.Id == currency);
    return refundQuery.Sum(r => r.Amount);
  }
}

// Helper classes for chart structure
