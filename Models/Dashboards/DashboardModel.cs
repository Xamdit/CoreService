using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;

namespace Service.Models.Dashboards;

public class DashboardModel(MyInstance self, MyContext db) : MyModel(self)
{
  // Get upcoming events this week
  public List<Event> GetUpcomingEvents()
  {
    var mondayThisWeek = self.helper.first_day_of_this_week();
    // var mondayThisWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
    var sundayThisWeek = mondayThisWeek.AddDays(6);
    var current_user_id = self.input.session.get<int>("current_user_id");
    return db.Events
      .Where(e =>
        DateTime.Parse(e.Start) >= mondayThisWeek && DateTime.Parse(e.Start) <= sundayThisWeek &&
        (e.UserId == current_user_id || e.Public == 1))
      .OrderByDescending(e => e.Start)
      .Take(6)
      .ToList();
  }

  // Get total upcoming events next week
  public int GetUpcomingEventsNextWeek()
  {
    var mondayNextWeek = self.helper.get_monday_this_week();
    var sundayNextWeek = mondayNextWeek.AddDays(6);
    var current_user_id = self.input.session.get<int>("current_user_id");
    return db.Events
      .Count(e =>
        DateTime.Parse(e.Start) >= mondayNextWeek &&
        DateTime.Parse(e.Start) <= sundayNextWeek &&
        (e.UserId == current_user_id || e.Public == 1));
  }

  // Weekly payments statistics (chart data)
  public Chart GetWeeklyPaymentsStatistics(int? currency)
  {
    var chart = new Chart
    {
      Labels = GetWeekdays(),
      Datasets = new List<ChartDataset>
      {
        new() { Label = "This Week Payments", Data = new List<int>(new int[7]) },
        new() { Label = "Last Week Payments", Data = new List<int>(new int[7]) }
      }
    };

    var payments = GetPaymentsByWeek(currency, DateTime.Now);
    var lastWeekPayments = GetPaymentsByWeek(currency, DateTime.Now.AddDays(-7));

    FillPaymentData(chart.Datasets[0].Data, payments);
    FillPaymentData(chart.Datasets[1].Data, lastWeekPayments);

    return chart;
  }

  private List<InvoicePaymentRecord> GetPaymentsByWeek(int? currency, DateTime weekStartDate)
  {
    // var startOfWeek = weekStartDate.StartOfWeek(DayOfWeek.Monday);
    var startOfWeek = self.helper.first_day_of_this_week();

    var endOfWeek = startOfWeek.AddDays(6);

    var query = db.InvoicePaymentRecords
      .Include(p => p.Invoice)
      .Where(p =>
        DateTime.Parse(p.Date) >= startOfWeek &&
        DateTime.Parse(p.Date) <= endOfWeek && p.Invoice.Status != 5);

    if (currency.HasValue) query = query.Where(p => p.Invoice.Currency.Id == currency.Value);

    return query.ToList();
  }

  private void FillPaymentData(IList<int> data, List<InvoicePaymentRecord> payments)
  {
    var weekdays = GetWeekdaysOriginal();
    foreach (var payment in payments)
    {
      var paymentDay = DateTime.Parse(payment.Date).DayOfWeek;
      var index = weekdays.IndexOf(paymentDay.ToString());
      if (index >= 0) data[index] += Convert.ToInt32(payment.Amount);
    }
  }

  // Monthly payments statistics (chart data)
  public Chart GetMonthlyPaymentsStatistics(int? currency)
  {
    var payments = db.InvoicePaymentRecords
      .Include(p => p.Invoice)
      .Where(p => DateTime.Parse(p.Date).Year == DateTime.Now.Year && p.Invoice.Status != 5)
      .GroupBy(p => DateTime.Parse(p.Date).Month)
      .Select(g => new PaymentData { Month = g.Key, Total = g.Sum(p => Convert.ToInt32(p.Amount)) })
      .ToList();

    // var chart = new Chart
    // {
    //   Labels = Enumerable.Range(1, 12).Select(i => DateTimeFormatInfo.CurrentInfo.GetMonthName(i)).ToList(),
    //   Datasets = new List<Dataset>
    //   {
    //     new()
    //     {
    //       Label = "Income",
    //       Data = Enumerable.Range(1, 12)
    //         .Select(i => payments.FirstOrDefault(p => p.Month == i)!.Total)
    //         .ToList()
    //     }
    //   }
    // };
    var chart = new Chart
    {
      Labels = Enumerable.Range(1, 12)
        .Select(i => DateTimeFormatInfo.CurrentInfo?.GetMonthName(i) ?? $"Month {i}")
        .ToList(),
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = "Income",
          Data = Enumerable.Range(1, 12)
            .Select(i => payments.FirstOrDefault(p => p.Month == i)?.Total ?? 0)
            .ToList(),
          BackgroundColor = "rgba(75, 192, 192, 0.2)",
          BorderColor = "rgba(75, 192, 192, 1)",
          BorderWidth = 1,
          Tension = false // Adjusted to match C# type (bool instead of string or other mismatched type)
        }
      }
    };

    return chart;
  }

  // Helper methods to get weekdays, start of week etc.
  private List<string> GetWeekdays()
  {
    return new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
  }

  private List<string> GetWeekdaysOriginal()
  {
    return new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
  }
}
