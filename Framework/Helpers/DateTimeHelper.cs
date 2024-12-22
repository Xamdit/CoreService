using Service.Framework.Core.Engine;

namespace Service.Framework.Helpers;

public static class DateTimeHelper
{
  public static DateTime first_day_of_this_month(this HelperBase self)
  {
    return DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
  }

  public static DateTime first_day_of_previous_month(this HelperBase self)
  {
    var previousMonthFirstDay = DateTime.Now.AddMonths(-1).Date;
    return DateTime.Parse(previousMonthFirstDay.ToString("yyyy-MM-01"));
  }

  public static DateTime last_day_of_previous_month(this HelperBase self)
  {
    var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    return firstDayOfMonth.AddDays(-1);
  }

  public static DateTime last_day_of_this_month(this HelperBase self)
  {
    var lastDayOfMonth = new DateTime(
      DateTime.Now.Year,
      DateTime.Now.Month,
      DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
    return DateTime.Parse(lastDayOfMonth.ToString("yyyy-MM-dd") + " 23:59:59");
  }

  public static DateTime first_day_of_this_week(this HelperBase self)
  {
    var now = DateTime.Now;
    var mondayThisWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
    return DateTime.Parse(mondayThisWeek.ToString("yyyy-MM-dd"));
  }

  public static DateTime last_day_of_this_week(this HelperBase self)
  {
    var now = DateTime.Now;
    var sundayThisWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Sunday);
    var endOfDay = sundayThisWeek.Date.AddDays(1).AddTicks(-1);
    return DateTime.Parse(endOfDay.ToString("yyyy-MM-dd HH:mm:ss"));
  }

  public static DateTime first_day_of_previous_week(this HelperBase self)
  {
    var mondayPreviousWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday - 7);
    return DateTime.Parse(mondayPreviousWeek.ToString("yyyy-MM-dd"));
  }

  public static DateTime last_day_of_previous_week(this HelperBase self)
  {
    var sundayPreviousWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Sunday - 7);
    var endOfDay = sundayPreviousWeek.Date.AddDays(1).AddTicks(-1);
    return DateTime.Parse(endOfDay.ToString("yyyy-MM-dd HH:mm:ss"));
  }

  public static DateTime get_monday_this_week(this HelperBase self)
  {
    var today = DateTime.Today;
    var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
    return today.AddDays(daysUntilMonday);
  }

  public static DateTime get_sunday_this_week(this HelperBase self)
  {
    var mondayThisWeek = self.get_monday_this_week();
    return mondayThisWeek.AddDays(6);
  }

  public static DateTime get_monday_previous_week(this HelperBase self)
  {
    var mondayThisWeek = self.get_monday_this_week();
    return mondayThisWeek.AddDays(-7);
  }

  public static DateTime get_sunday_previous_week(this HelperBase self)
  {
    var mondayPreviousWeek = self.get_monday_previous_week();
    return mondayPreviousWeek.AddDays(6);
  }

  public static DateTime first_day_of_this_year(this HelperBase self)
  {
    return new DateTime(DateTime.Now.Year, 1, 1);
  }

  public static DateTime last_day_of_this_year(this HelperBase self)
  {
    var firstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);
    return firstDayOfYear.AddYears(1).AddDays(-1);
  }

  public static DateTime first_day_of_previous_year(this HelperBase self)
  {
    var firstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);
    return firstDayOfYear.AddYears(-1);
  }

  public static DateTime last_day_of_previous_year(this HelperBase self)
  {
    var firstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);
    return firstDayOfYear.AddDays(-1);
  }

  public static DateTime human_to_unix(this HelperBase self, string datetime)
  {
    var date = DateTime.Parse(datetime);
    var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    return unixTime.AddSeconds(date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
  }

  public static string time_ago(this HelperBase self, DateTime? targetTime)
  {
    var now = DateTime.Now;
    var ts = now - targetTime!.Value;
    if (ts.TotalSeconds < 60) return "Just now";
    if (ts.TotalMinutes < 60) return $"{Math.Floor(ts.TotalMinutes)} minute{pluralize(Math.Floor(ts.TotalMinutes))} ago";
    if (ts.TotalHours < 24) return $"{Math.Floor(ts.TotalHours)} hour{pluralize(Math.Floor(ts.TotalHours))} ago";
    return ts.TotalDays switch
    {
      < 7 => $"{Math.Floor(ts.TotalDays)} day{pluralize(Math.Floor(ts.TotalDays))} ago",
      < 365 => $"{Math.Floor(ts.TotalDays / 7)} week{pluralize(Math.Floor(ts.TotalDays / 7))} ago",
      _ => $"{Math.Floor(ts.TotalDays / 365)} year{pluralize(Math.Floor(ts.TotalDays / 365))} ago"
    };

    string pluralize(double value)
    {
      return value == 1 ? string.Empty : "s";
    }
  }

  public static List<string> get_weekdays(this HelperBase self, DateTime startDate = default, DateTime endDate = default)
  {
    var weekdays = new List<string>();
    var currentDate = startDate;
    while (currentDate <= endDate)
    {
      if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
        weekdays.Add(currentDate.ToString());
      currentDate = currentDate.AddDays(1);
    }

    return weekdays;
  }

  public static List<DateTime> get_weekdays_between_dates(this HelperBase self, DateTime startDate, DateTime endDate)
  {
    var weekdays = new List<DateTime>();
    for (var date = startDate; date <= endDate; date = date.AddDays(1))
      if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
        weekdays.Add(date);
    return weekdays;
  }

  public static string split_weeks_chart_label(this HelperBase self, List<DateTime> weeks, int week)
  {
    return string.Empty;
  }

  public static string _d(this HelperBase helper, DateTime? value)
  {
    return !value.HasValue ? value!.Value.ToString("dd/MM/yyyy") : string.Empty;
  }

  public static TimeSpan diff(this HelperBase self, DateTime startDateTime, DateTime endDateTime)
  {
    return endDateTime - startDateTime;
  }
}
