using System.Globalization;

namespace TVClock.Tests;

public class TimeFormatterTests
{
    [Theory]
    [InlineData(0, 0, "00:00")]
    [InlineData(9, 5, "09:05")]
    [InlineData(12, 0, "12:00")]
    [InlineData(13, 30, "13:30")]
    [InlineData(23, 59, "23:59")]
    public void Format_時と分を2桁ずつの24時間表記にする(int hour, int minute, string expected)
    {
        var time = new DateTime(2026, 1, 1, hour, minute, 0);

        Assert.Equal(expected, TimeFormatter.Format(time));
    }

    [Fact]
    public void Format_秒は表示に含めない()
    {
        var time = new DateTime(2026, 1, 1, 9, 5, 59);

        Assert.Equal("09:05", TimeFormatter.Format(time));
    }

    [Theory]
    [InlineData("ja-JP")]
    [InlineData("en-US")]
    [InlineData("fi-FI")]
    [InlineData("ar-SA")]
    public void Format_地域設定が変わっても同じ表示になる(string cultureName)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            var time = new DateTime(2026, 1, 1, 9, 5, 0);

            Assert.Equal("09:05", TimeFormatter.Format(time));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
