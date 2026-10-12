using System.Runtime.InteropServices;

namespace TVClock;

/// <summary>
/// Win32 API でウィンドウを操作する。P/Invoke はこのクラスにまとめ、画面のコードからは直接呼ばない
/// </summary>
public static class NativeWindow
{
    private const int GwlExStyle = -20;

    // マウス操作を受け取らず、下のウィンドウへ通す
    private const long WsExTransparent = 0x00000020;

    // 透過ウィンドウ。WS_EX_TRANSPARENT でマウス操作を通すために必要
    private const long WsExLayered = 0x00080000;

    /// <summary>
    /// ウィンドウのクリックスルー（マウス操作を下のウィンドウへ通す状態）を切り替える
    /// </summary>
    /// <param name="handle">ウィンドウのハンドル</param>
    /// <param name="enabled">クリックスルーにするときは true</param>
    public static void SetClickThrough(IntPtr handle, bool enabled)
    {
        var style = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        var newStyle = WithClickThrough(style, enabled);
        if (newStyle != style)
        {
            SetWindowLongPtr(handle, GwlExStyle, new IntPtr(newStyle));
        }
    }

    /// <summary>
    /// 拡張ウィンドウスタイルに、クリックスルーの有無を反映した値を返す。ほかのスタイルは変えない
    /// </summary>
    public static long WithClickThrough(long exStyle, bool enabled) =>
        enabled
            ? exStyle | WsExTransparent | WsExLayered
            // 透過ウィンドウであることは時計の表示にも必要なので、WS_EX_LAYERED は外さない
            : exStyle & ~WsExTransparent;

    // 32 ビットのプロセスには GetWindowLongPtr / SetWindowLongPtr がないため、呼び分ける
    private static IntPtr GetWindowLongPtr(IntPtr handle, int index) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(handle, index) : new IntPtr(GetWindowLong32(handle, index));

    private static IntPtr SetWindowLongPtr(IntPtr handle, int index, IntPtr value) =>
        IntPtr.Size == 8
            ? SetWindowLongPtr64(handle, index, value)
            : new IntPtr(SetWindowLong32(handle, index, value.ToInt32()));

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong32(IntPtr handle, int index);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr handle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong32(IntPtr handle, int index, int value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr handle, int index, IntPtr value);
}
