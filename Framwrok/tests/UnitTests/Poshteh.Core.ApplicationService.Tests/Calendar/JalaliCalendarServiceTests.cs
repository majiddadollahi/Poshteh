using Poshteh.Core.Calendar;

namespace Poshteh.Core.ApplicationService.Tests.Calendar;

public sealed class JalaliCalendarServiceTests
{
    [Fact]
    public void NullInputs_ReturnNull()
    {
        Assert.Null(JalaliCalendarService.FormatDate((DateOnly?)null));
        Assert.Null(JalaliCalendarService.FormatDate((DateTime?)null));
        Assert.Null(JalaliCalendarService.FormatDateTime(null));
        Assert.Null(JalaliCalendarService.FormatDateTimeWithSeconds(null));
    }

    [Fact]
    public void FormatDate_DateOnly_UsesFourDigitYearAndTwoDigitMonthDay()
    {
        Assert.Equal("1403/01/01", JalaliCalendarService.FormatDate(new DateOnly(2024, 3, 20)));
        Assert.Equal("1403/10/26", JalaliCalendarService.FormatDate(new DateOnly(2025, 1, 15)));
    }

    [Fact]
    public void FormatDate_DateOnly_DoesNotConvertTimeZone()
    {
        // Same calendar date as the UTC instant below, but a DateOnly never shifts to the next day.
        Assert.Equal("1403/10/26", JalaliCalendarService.FormatDate(new DateOnly(2025, 1, 15)));
    }

    [Fact]
    public void UtcNearMidnight_MovesToNextDayInIranTime()
    {
        var utc = new DateTime(2025, 1, 15, 21, 0, 0, DateTimeKind.Utc); // 00:30 on Jan 16 in Tehran

        Assert.Equal("1403/10/27", JalaliCalendarService.FormatDate(utc));
        Assert.Equal("1403/10/27 00:30", JalaliCalendarService.FormatDateTime(utc));
        Assert.Equal("1403/10/27 00:30:00", JalaliCalendarService.FormatDateTimeWithSeconds(utc));
    }

    [Fact]
    public void UtcBeforeIranMidnight_StaysOnSameDay()
    {
        var utc = new DateTime(2025, 1, 15, 20, 0, 0, DateTimeKind.Utc);

        Assert.Equal("1403/10/26 23:30", JalaliCalendarService.FormatDateTime(utc));
    }

    [Fact]
    public void FormatDateTimeWithSeconds_IncludesSeconds()
    {
        var utc = new DateTime(2025, 1, 15, 21, 5, 9, DateTimeKind.Utc);

        Assert.Equal("1403/10/27 00:35:09", JalaliCalendarService.FormatDateTimeWithSeconds(utc));
    }

    [Fact]
    public void UnspecifiedKind_IsTreatedAsUtc()
    {
        var utc = new DateTime(2025, 1, 15, 21, 0, 0, DateTimeKind.Utc);
        var unspecified = DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);

        Assert.Equal(JalaliCalendarService.FormatDateTime(utc), JalaliCalendarService.FormatDateTime(unspecified));
    }

    [Fact]
    public void LocalKind_IsConvertedToUtcFirst()
    {
        var utc = new DateTime(2025, 1, 15, 21, 0, 0, DateTimeKind.Utc);

        Assert.Equal(
            JalaliCalendarService.FormatDateTimeWithSeconds(utc),
            JalaliCalendarService.FormatDateTimeWithSeconds(utc.ToLocalTime()));
    }

    [Fact]
    public void DisplayFields_AppendJalaliSuffix()
    {
        Assert.Equal("Jalali", JalaliDisplayFields.Suffix);
        Assert.Equal("CreatedAtJalali", JalaliDisplayFields.PropertyNameFor("CreatedAt"));
    }
}
