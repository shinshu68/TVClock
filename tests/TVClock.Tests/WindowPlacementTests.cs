using System.Windows;

namespace TVClock.Tests;

public class WindowPlacementTests
{
    // 1920x1080 のモニターの左に、同じ大きさのモニターをもう 1 台並べた領域
    private static readonly Rect Screen = new(-1920, 0, 3840, 1080);

    [Theory]
    [InlineData(40, 40)]
    [InlineData(-1900, 500)]
    [InlineData(1600, 950)]
    public void IsOnScreen_全体が画面内にあれば見えている(double left, double top)
    {
        Assert.True(WindowPlacement.IsOnScreen(new Rect(left, top, 300, 120), Screen));
    }

    [Theory]
    [InlineData(1920, 40)]
    [InlineData(3000, 40)]
    [InlineData(-2300, 40)]
    [InlineData(40, 1080)]
    [InlineData(40, -200)]
    public void IsOnScreen_画面の外にあれば見えていない(double left, double top)
    {
        Assert.False(WindowPlacement.IsOnScreen(new Rect(left, top, 300, 120), Screen));
    }

    [Theory]
    [InlineData(1880, 40, true)]
    [InlineData(1881, 40, false)]
    [InlineData(40, 1040, true)]
    [InlineData(40, 1041, false)]
    public void IsOnScreen_端からはみ出していても40以上見えていれば見えている(double left, double top, bool expected)
    {
        Assert.Equal(expected, WindowPlacement.IsOnScreen(new Rect(left, top, 300, 120), Screen));
    }

    [Fact]
    public void IsOnScreen_40より小さいウィンドウは全体が画面内なら見えている()
    {
        Assert.True(WindowPlacement.IsOnScreen(new Rect(100, 100, 20, 10), Screen));
    }

    [Theory]
    [InlineData(double.NaN, 40)]
    [InlineData(40, double.NaN)]
    [InlineData(double.PositiveInfinity, 40)]
    [InlineData(40, double.NegativeInfinity)]
    public void IsOnScreen_位置が数値として扱えなければ見えていない(double left, double top)
    {
        Assert.False(WindowPlacement.IsOnScreen(new Rect(left, top, 300, 120), Screen));
    }
}
