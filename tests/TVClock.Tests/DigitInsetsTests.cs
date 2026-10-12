using System.Windows;
using System.Windows.Media;

namespace TVClock.Tests;

public class DigitInsetsTests
{
    // Windows に標準で入っているフォントで確かめる
    private static readonly Typeface Typeface = new("Segoe UI");

    [Theory]
    [InlineData(24)]
    [InlineData(96)]
    [InlineData(300)]
    public void Measure_すき間は0以上の整数で返す(double fontSize)
    {
        var insets = DigitInsets.Measure(Typeface, fontSize, pixelsPerDip: 1);

        foreach (var inset in new[] { insets.Left, insets.Top, insets.Right, insets.Bottom })
        {
            Assert.True(inset >= 0);
            Assert.Equal(Math.Floor(inset), inset);
        }
    }

    [Fact]
    public void Measure_数字の上下にはすき間がある()
    {
        var insets = DigitInsets.Measure(Typeface, 96, pixelsPerDip: 1);

        Assert.True(insets.Top > 0);
        Assert.True(insets.Bottom > 0);
    }

    [Fact]
    public void Measure_すき間を詰めても数字の高さが残る()
    {
        const double fontSize = 96;

        var insets = DigitInsets.Measure(Typeface, fontSize, pixelsPerDip: 1);

        // 数字の高さは文字サイズの半分以上ある
        var lineHeight = Typeface.FontFamily.LineSpacing * fontSize;
        Assert.True(lineHeight - insets.Top - insets.Bottom > fontSize / 2);
    }

    [Fact]
    public void Measure_文字サイズが大きいほど上下のすき間も大きい()
    {
        var small = DigitInsets.Measure(Typeface, 48, pixelsPerDip: 1);
        var large = DigitInsets.Measure(Typeface, 192, pixelsPerDip: 1);

        Assert.True(large.Top > small.Top);
        Assert.True(large.Bottom > small.Bottom);
    }
}
