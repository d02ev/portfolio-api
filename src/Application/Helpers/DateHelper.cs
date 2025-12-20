using System.Globalization;

namespace Application.Helpers;

public static class DateHelper
{
  public static string ConvertDateToMonthYearString(DateTime date)
  {
    date = date.AddDays(15);
    return date.ToString("MMM yyyy", CultureInfo.InvariantCulture);
  }

  public static DateTime ConvertMonthYearStringToDate(string monthYearString)
  {
    return DateTime.ParseExact(monthYearString, "MMM yyyy", CultureInfo.InvariantCulture).AddDays(15);
  }
}