using System.Windows;

namespace TVClock.Tests;

public class NativeWindowTests
{
    private const long WsExTransparent = 0x00000020;
    private const long WsExLayered = 0x00080000;
    private const long WsExToolWindow = 0x00000080;

    // クリックスルーと関係のないスタイル (WS_EX_TOPMOST)
    private const long WsExTopmost = 0x00000008;

    private const long WsCaption = 0x00C00000;
    private const long WsPopup = 0x80000000;

    // 枠だけがあるウィンドウ (WS_BORDER)。WS_CAPTION の一部のビットだけが立っている
    private const long WsBorder = 0x00800000;

    // 1920x1080 のモニターと、その右に並べた 2 台目のモニター
    private static readonly Int32Rect Monitor = new(0, 0, 1920, 1080);
    private static readonly Int32Rect SecondMonitor = new(1920, 0, 1920, 1080);

    [Fact]
    public void IsFullscreen_タイトルバーがなくモニター全体を覆っていれば全画面表示()
    {
        Assert.True(NativeWindow.IsFullscreen(new Int32Rect(0, 0, 1920, 1080), Monitor, WsPopup));
    }

    [Fact]
    public void IsFullscreen_2台目のモニター全体を覆っていれば全画面表示()
    {
        Assert.True(NativeWindow.IsFullscreen(new Int32Rect(1920, 0, 1920, 1080), SecondMonitor, WsPopup));
    }

    [Fact]
    public void IsFullscreen_枠だけがあるウィンドウも全画面表示とみなす()
    {
        Assert.True(NativeWindow.IsFullscreen(new Int32Rect(0, 0, 1920, 1080), Monitor, WsPopup | WsBorder));
    }

    [Fact]
    public void IsFullscreen_最大化したウィンドウはモニターを覆っていても全画面表示ではない()
    {
        // 最大化したウィンドウは、枠の分だけモニターからはみ出す
        var maximized = new Int32Rect(-8, -8, 1936, 1096);

        Assert.False(NativeWindow.IsFullscreen(maximized, Monitor, WsCaption));
    }

    [Theory]
    [InlineData(0, 0, 1920, 1040)]
    [InlineData(100, 100, 800, 600)]
    [InlineData(1, 0, 1920, 1080)]
    public void IsFullscreen_モニター全体を覆っていなければ全画面表示ではない(int x, int y, int width, int height)
    {
        Assert.False(NativeWindow.IsFullscreen(new Int32Rect(x, y, width, height), Monitor, WsPopup));
    }

    [Fact]
    public void IsFullscreen_隣のモニターを覆っているウィンドウは全画面表示ではない()
    {
        Assert.False(NativeWindow.IsFullscreen(new Int32Rect(1920, 0, 1920, 1080), Monitor, WsPopup));
    }

    [Fact]
    public void WithClickThrough_有効にするとマウス操作を通すスタイルが付く()
    {
        var style = NativeWindow.WithClickThrough(WsExTopmost, enabled: true);

        Assert.Equal(WsExTopmost | WsExTransparent | WsExLayered, style);
    }

    [Fact]
    public void WithClickThrough_無効にするとマウス操作を通すスタイルだけが外れる()
    {
        var style = NativeWindow.WithClickThrough(WsExTopmost | WsExTransparent | WsExLayered, enabled: false);

        Assert.Equal(WsExTopmost | WsExLayered, style);
    }

    [Fact]
    public void WithToolWindow_ほかのスタイルを残したままツールウィンドウのスタイルが付く()
    {
        var style = NativeWindow.WithToolWindow(WsExTopmost | WsExLayered);

        Assert.Equal(WsExTopmost | WsExLayered | WsExToolWindow, style);
    }

    [Fact]
    public void WithClickThrough_ツールウィンドウのスタイルは変えない()
    {
        var style = WsExTopmost | WsExLayered | WsExToolWindow;

        var enabled = NativeWindow.WithClickThrough(style, enabled: true);
        var disabled = NativeWindow.WithClickThrough(enabled, enabled: false);

        Assert.Equal(style | WsExTransparent, enabled);
        Assert.Equal(style, disabled);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithClickThrough_同じ状態をもう一度指定しても変わらない(bool enabled)
    {
        var once = NativeWindow.WithClickThrough(WsExTopmost | WsExLayered, enabled);

        Assert.Equal(once, NativeWindow.WithClickThrough(once, enabled));
    }
}
