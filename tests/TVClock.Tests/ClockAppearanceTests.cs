namespace TVClock.Tests;

public class ClockAppearanceTests
{
    [Fact]
    public void NormalizeFontSize_未保存のときは既定値を返す()
    {
        Assert.Equal(ClockAppearance.DefaultFontSize, ClockAppearance.NormalizeFontSize(null));
    }

    [Theory]
    [InlineData(24)]
    [InlineData(96)]
    [InlineData(100.5)]
    [InlineData(300)]
    public void NormalizeFontSize_範囲内の値はそのまま返す(double value)
    {
        Assert.Equal(value, ClockAppearance.NormalizeFontSize(value));
    }

    [Theory]
    [InlineData(23.9)]
    [InlineData(0)]
    [InlineData(-50)]
    public void NormalizeFontSize_小さすぎる値は最小値に丸める(double value)
    {
        Assert.Equal(ClockAppearance.MinFontSize, ClockAppearance.NormalizeFontSize(value));
    }

    [Theory]
    [InlineData(300.1)]
    [InlineData(100000)]
    public void NormalizeFontSize_大きすぎる値は最大値に丸める(double value)
    {
        Assert.Equal(ClockAppearance.MaxFontSize, ClockAppearance.NormalizeFontSize(value));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NormalizeFontSize_数値として不正なときは既定値を返す(double value)
    {
        Assert.Equal(ClockAppearance.DefaultFontSize, ClockAppearance.NormalizeFontSize(value));
    }

    [Fact]
    public void NormalizeBackgroundOpacity_未保存のときは既定値を返す()
    {
        Assert.Equal(ClockAppearance.DefaultBackgroundOpacity, ClockAppearance.NormalizeBackgroundOpacity(null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.35)]
    [InlineData(1)]
    public void NormalizeBackgroundOpacity_範囲内の値はそのまま返す(double value)
    {
        Assert.Equal(value, ClockAppearance.NormalizeBackgroundOpacity(value));
    }

    [Theory]
    [InlineData(-0.1, 0)]
    [InlineData(-5, 0)]
    [InlineData(1.1, 1)]
    [InlineData(50, 1)]
    public void NormalizeBackgroundOpacity_範囲外の値は範囲内に丸める(double value, double expected)
    {
        Assert.Equal(expected, ClockAppearance.NormalizeBackgroundOpacity(value));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NormalizeBackgroundOpacity_数値として不正なときは既定値を返す(double value)
    {
        Assert.Equal(ClockAppearance.DefaultBackgroundOpacity, ClockAppearance.NormalizeBackgroundOpacity(value));
    }

    [Fact]
    public void 既定値は範囲内で刻みの位置にある()
    {
        // 設定画面のスライダーは最小値から刻みごとに止まるため、既定値がその位置にないと表示とずれる
        Assert.InRange(ClockAppearance.DefaultFontSize, ClockAppearance.MinFontSize, ClockAppearance.MaxFontSize);
        Assert.Equal(0, (ClockAppearance.DefaultFontSize - ClockAppearance.MinFontSize) % ClockAppearance.FontSizeStep);
        Assert.Equal(0, (ClockAppearance.MaxFontSize - ClockAppearance.MinFontSize) % ClockAppearance.FontSizeStep);
        Assert.InRange(
            ClockAppearance.DefaultBackgroundOpacity,
            ClockAppearance.MinBackgroundOpacity,
            ClockAppearance.MaxBackgroundOpacity);
    }
}
