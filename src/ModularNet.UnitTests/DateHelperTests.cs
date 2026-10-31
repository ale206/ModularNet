using ModularNet.Shared.Helpers;

namespace ModularNet.UnitTests;

public class DateHelperTests
{
    [Fact]
    public void UnixTimeStampToDateTimeConvertsEpochCorrectly()
    {
        // Arrange
        const double unixTimeStamp = 0;

        // Act
        var dateTime = DateHelper.UnixTimeStampToDateTime(unixTimeStamp);

        // Assert
        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), dateTime);
    }

    [Fact]
    public void UnixTimeStampToDateTimeConvertsPositiveTimestamp()
    {
        // Arrange
        const double unixTimeStamp = 1704067200; // 2024-01-01 00:00:00 UTC

        // Act
        var dateTime = DateHelper.UnixTimeStampToDateTime(unixTimeStamp);

        // Assert
        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), dateTime);
    }

    [Fact]
    public void DifferenceBetweenTwoDatesInDaysReturnsCorrectDifference()
    {
        // Arrange
        var firstDate = new DateTime(2025, 1, 10);
        var secondDate = new DateTime(2025, 1, 5);

        // Act
        var difference = DateHelper.DifferenceBetweenTwoDatesInDays(firstDate, secondDate);

        // Assert
        Assert.Equal(5, difference);
    }

    [Fact]
    public void SecondsUntilMidnightReturnsPositiveSeconds()
    {
        // Arrange & Act
        var seconds = DateHelper.SecondsUntilMidnight();

        // Assert
        Assert.InRange(seconds, 1, 86400);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void GetMonthNameReturnsAbbreviatedMonth(int monthNumber)
    {
        // Arrange
        var expectedMonthName = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(monthNumber);

        // Act
        var monthName = DateHelper.GetMonthName(monthNumber);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(monthName));
        Assert.Equal(expectedMonthName, monthName);
    }
}
