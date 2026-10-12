namespace TVClock.Tests;

public class NativeWindowTests
{
    private const long WsExTransparent = 0x00000020;
    private const long WsExLayered = 0x00080000;

    // クリックスルーと関係のないスタイル（WS_EX_TOPMOST）
    private const long WsExTopmost = 0x00000008;

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithClickThrough_同じ状態をもう一度指定しても変わらない(bool enabled)
    {
        var once = NativeWindow.WithClickThrough(WsExTopmost | WsExLayered, enabled);

        Assert.Equal(once, NativeWindow.WithClickThrough(once, enabled));
    }
}
