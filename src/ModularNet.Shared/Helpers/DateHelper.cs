using System.Globalization;

namespace ModularNet.Shared.Helpers;

public static class DateHelper
{
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }

    public static int DifferenceBetweenTwoDatesInDays(DateTime firstDate, DateTime secondDate)
    {
        var remainingTime = Convert.ToDateTime(firstDate).Date - secondDate.Date;
        return (int)remainingTime.TotalDays;
    }

    public static int SecondsUntilMidnight()
    {
        var now = DateTime.UtcNow;
        var midnight = now.Date.AddDays(1); // Next midnight

        var timeUntilMidnight = midnight - now;
        var secondsUntilMidnight = (int)timeUntilMidnight.TotalSeconds;

        return secondsUntilMidnight;
    }

    public static string GetMonthName(int monthNumber)
    {
        var dateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;
        return dateTimeFormatInfo.GetAbbreviatedMonthName(monthNumber);
    }
}