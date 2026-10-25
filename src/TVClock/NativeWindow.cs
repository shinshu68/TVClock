using System.Runtime.InteropServices;
using System.Windows;

namespace TVClock;

/// <summary>
/// Win32 API でウィンドウを操作する。P/Invoke はこのクラスにまとめ、画面のコードからは直接呼ばない
/// </summary>
public static class NativeWindow
{
    private const int GwlStyle = -16;
    private const int GwlExStyle = -20;

    /// <summary>
    /// タイトルバーのあるウィンドウ。全画面表示のアプリは、ふつうタイトルバーを外している
    /// </summary>
    private const long WsCaption = 0x00C00000;

    /// <summary>
    /// MonitorFromWindow で、ウィンドウにいちばん近いモニターを返させる
    /// </summary>
    private const uint MonitorDefaultToNearest = 2;

    /// <summary>
    /// デスクトップ (壁紙とアイコン) を表示しているウィンドウのクラス名。画面全体を覆うが、アプリの全画面表示ではない
    /// </summary>
    private static readonly string[] DesktopClassNames = ["Progman", "WorkerW"];

    /// <summary>
    /// マウス操作を受け取らず、下のウィンドウへ通す
    /// </summary>
    private const long WsExTransparent = 0x00000020;

    /// <summary>
    /// 透過ウィンドウ。WS_EX_TRANSPARENT でマウス操作を通すために必要
    /// </summary>
    private const long WsExLayered = 0x00080000;

    /// <summary>
    /// ツールウィンドウ。Alt+Tab の切り替えの一覧に出なくなる
    /// </summary>
    private const long WsExToolWindow = 0x00000080;

    /// <summary>
    /// ウィンドウのクリックスルー (マウス操作を下のウィンドウへ通す状態) を切り替える
    /// </summary>
    /// <param name="handle">ウィンドウのハンドル</param>
    /// <param name="enabled">クリックスルーにするときは true</param>
    public static void SetClickThrough(IntPtr handle, bool enabled)
    {
        ChangeExStyle(handle, style => WithClickThrough(style, enabled));
    }

    /// <summary>
    /// ウィンドウを Alt+Tab の切り替えの一覧に出さないようにする
    /// </summary>
    /// <param name="handle">ウィンドウのハンドル</param>
    public static void HideFromTaskSwitcher(IntPtr handle)
    {
        ChangeExStyle(handle, WithToolWindow);
    }

    /// <summary>
    /// 拡張ウィンドウスタイルに、ツールウィンドウのスタイルを足した値を返す。ほかのスタイルは変えない
    /// </summary>
    public static long WithToolWindow(long exStyle) => exStyle | WsExToolWindow;

    /// <summary>
    /// 拡張ウィンドウスタイルに、クリックスルーの有無を反映した値を返す。ほかのスタイルは変えない
    /// </summary>
    public static long WithClickThrough(long exStyle, bool enabled) =>
        enabled
            ? exStyle | WsExTransparent | WsExLayered
            // 透過ウィンドウであることは時計の表示にも必要なので、WS_EX_LAYERED は外さない
            : exStyle & ~WsExTransparent;

    /// <summary>
    /// 自分のウィンドウと同じモニターで、ほかのアプリが全画面表示で手前にあるかを返す
    /// </summary>
    /// <param name="ownHandle">自分のウィンドウのハンドル</param>
    public static bool IsFullscreenAppInFront(IntPtr ownHandle)
    {
        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero || foreground == GetShellWindow() || foreground == GetDesktopWindow())
        {
            return false;
        }

        // 設定画面など、自分のウィンドウを操作している間は隠さない
        GetWindowThreadProcessId(foreground, out var processId);
        if (processId == (uint)Environment.ProcessId)
        {
            return false;
        }

        var className = new char[64];
        var length = GetClassName(foreground, className, className.Length);
        if (DesktopClassNames.Contains(new string(className, 0, length)))
        {
            return false;
        }

        var monitor = MonitorFromWindow(foreground, MonitorDefaultToNearest);
        if (monitor != MonitorFromWindow(ownHandle, MonitorDefaultToNearest))
        {
            return false;
        }

        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetWindowRect(foreground, out var window) || !GetMonitorInfo(monitor, ref info))
        {
            return false;
        }

        return IsFullscreen(
            ToInt32Rect(window),
            ToInt32Rect(info.Monitor),
            GetWindowLongPtr(foreground, GwlStyle).ToInt64());
    }

    /// <summary>
    /// ウィンドウが全画面表示かを返す。モニター全体を覆っていて、タイトルバーがなければ全画面表示とみなす
    /// </summary>
    /// <remarks>
    /// 最大化しただけのウィンドウは、タスクバーを自動的に隠す設定だとモニター全体を覆うため、タイトルバーの有無で区別する
    /// </remarks>
    /// <param name="window">ウィンドウの位置と大きさ</param>
    /// <param name="monitor">ウィンドウがあるモニターの位置と大きさ</param>
    /// <param name="style">ウィンドウスタイル</param>
    public static bool IsFullscreen(Int32Rect window, Int32Rect monitor, long style)
    {
        var covers = window.X <= monitor.X
            && window.Y <= monitor.Y
            && window.X + window.Width >= monitor.X + monitor.Width
            && window.Y + window.Height >= monitor.Y + monitor.Height;

        return covers && (style & WsCaption) != WsCaption;
    }

    private static Int32Rect ToInt32Rect(NativeRect rect) =>
        new(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

    private static void ChangeExStyle(IntPtr handle, Func<long, long> change)
    {
        var style = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        var newStyle = change(style);
        if (newStyle != style)
        {
            SetWindowLongPtr(handle, GwlExStyle, new IntPtr(newStyle));
        }
    }

    // 32 ビットのプロセスには GetWindowLongPtr / SetWindowLongPtr がないため、呼び分ける
    private static IntPtr GetWindowLongPtr(IntPtr handle, int index) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(handle, index) : new IntPtr(GetWindowLong32(handle, index));

    private static IntPtr SetWindowLongPtr(IntPtr handle, int index, IntPtr value) =>
        IntPtr.Size == 8
            ? SetWindowLongPtr64(handle, index, value)
            : new IntPtr(SetWindowLong32(handle, index, value.ToInt32()));

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect WorkArea;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

    [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr handle, [Out] char[] className, int maxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr handle, out NativeRect rect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong32(IntPtr handle, int index);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr handle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong32(IntPtr handle, int index, int value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr handle, int index, IntPtr value);
}
