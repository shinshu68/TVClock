using WinForms = System.Windows.Forms;

namespace TVClock;

/// <summary>
/// タスクトレイのアイコンとそのメニュー。クリックスルー中は時計の上でマウス操作ができないため、ここから操作する
/// </summary>
public sealed class TrayIcon : IDisposable
{
    private readonly WinForms.NotifyIcon _notifyIcon;
    private readonly WinForms.ToolStripMenuItem _clickThroughItem;
    private readonly WinForms.ToolStripMenuItem _autoStartItem;

    public TrayIcon()
    {
        _clickThroughItem = new WinForms.ToolStripMenuItem("クリックスルー(&C)");
        _clickThroughItem.Click += (_, _) => ClickThroughClicked?.Invoke();

        _autoStartItem = new WinForms.ToolStripMenuItem("Windows の起動時に実行(&A)");
        _autoStartItem.Click += (_, _) => AutoStartClicked?.Invoke();

        var settingsItem = new WinForms.ToolStripMenuItem("設定(&S)...");
        settingsItem.Click += (_, _) => SettingsClicked?.Invoke();

        var exitItem = new WinForms.ToolStripMenuItem("終了(&X)");
        exitItem.Click += (_, _) => ExitClicked?.Invoke();

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add(_clickThroughItem);
        menu.Items.Add(_autoStartItem);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new WinForms.NotifyIcon
        {
            // 専用のアイコンができるまでは、Windows 標準のアプリアイコンを使う
            Icon = System.Drawing.SystemIcons.Application,
            Text = "TVClock",
            ContextMenuStrip = menu,
            Visible = true,
        };
    }

    /// <summary>
    /// メニューの「クリックスルー」が選ばれた
    /// </summary>
    public event Action? ClickThroughClicked;

    /// <summary>
    /// メニューの「Windows の起動時に実行」が選ばれた
    /// </summary>
    public event Action? AutoStartClicked;

    /// <summary>
    /// メニューの「設定」が選ばれた
    /// </summary>
    public event Action? SettingsClicked;

    /// <summary>
    /// メニューの「終了」が選ばれた
    /// </summary>
    public event Action? ExitClicked;

    /// <summary>
    /// メニューの「クリックスルー」のチェックを、現在の状態に合わせる
    /// </summary>
    public void ShowClickThrough(bool enabled)
    {
        _clickThroughItem.Checked = enabled;
    }

    /// <summary>
    /// メニューの「Windows の起動時に実行」のチェックを、現在の状態に合わせる
    /// </summary>
    public void ShowAutoStart(bool enabled)
    {
        _autoStartItem.Checked = enabled;
    }

    public void Dispose()
    {
        // 非表示にしてから破棄しないと、終了後もトレイにアイコンが残ることがある
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }
}
