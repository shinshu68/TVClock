using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace TVClock;

/// <summary>
/// 時刻（時:分）を表示する枠なし・最前面のウィンドウ
/// </summary>
public partial class MainWindow : Window
{
    // 表示は分単位だが、分の切り替わりが遅れないよう 1 秒ごとに確認する
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(1);

    private readonly DispatcherTimer _timer;
    private readonly SettingsStore _settingsStore = new(SettingsStore.DefaultPath);
    private readonly AppSettings _settings;
    private readonly TrayIcon _trayIcon = new();
    private readonly AutoStart _autoStart = new(
        AutoStart.DefaultKeyPath,
        AutoStart.DefaultValueName,
        Environment.ProcessPath ?? string.Empty);
    private SettingsWindow? _settingsWindow;
    private bool _clickThrough;

    public MainWindow()
    {
        InitializeComponent();

        _settings = _settingsStore.Load();
        ApplyFontSize(ClockAppearance.NormalizeFontSize(_settings.FontSize));
        ApplyBackgroundOpacity(ClockAppearance.NormalizeBackgroundOpacity(_settings.BackgroundOpacity));

        _timer = new DispatcherTimer { Interval = CheckInterval };
        _timer.Tick += (_, _) => UpdateTime();

        UpdateTime();
        _timer.Start();

        // ウィンドウの大きさは内容に合わせて決まるため、大きさが確定してから位置を復元する
        Loaded += (_, _) => RestorePosition();

        // クリックスルー中は時計の上で操作できないため、同じ操作をタスクトレイにも置く
        _trayIcon.ClickThroughClicked += () => SetClickThrough(!_clickThrough);
        _trayIcon.AutoStartClicked += ToggleAutoStart;
        _trayIcon.SettingsClicked += OpenSettings;
        _trayIcon.ExitClicked += Close;
        Closed += (_, _) => _trayIcon.Dispose();

        ShowAutoStart();
    }

    /// <summary>
    /// Win32 のウィンドウができてからでないと変えられない状態を反映する
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // 操作はタスクトレイからできるので、タスクバーにも Alt+Tab の一覧にも出さない
        NativeWindow.HideFromTaskSwitcher(new WindowInteropHelper(this).Handle);

        // 保存したクリックスルーの状態を復元する

        if (_settings.ClickThrough == true)
        {
            SetClickThrough(true);
        }
    }

    /// <summary>
    /// 左ボタンを押したままドラッグすると、ウィンドウを移動する
    /// </summary>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // ボタンが押されていない状態で DragMove を呼ぶと例外になる
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        // DragMove はボタンを離すまで戻らない
        DragMove();
        SavePosition();
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        OpenSettings();
    }

    private void OnClickThroughClick(object sender, RoutedEventArgs e)
    {
        SetClickThrough(!_clickThrough);
    }

    private void OnAutoStartClick(object sender, RoutedEventArgs e)
    {
        ToggleAutoStart();
    }

    /// <summary>
    /// 自動起動の登録を切り替える。登録できなかったときは、メニューのチェックは元のままになる
    /// </summary>
    private void ToggleAutoStart()
    {
        _autoStart.SetEnabled(!_autoStart.IsEnabled());
        ShowAutoStart();
    }

    /// <summary>
    /// メニューのチェックを、レジストリに登録されている状態に合わせる
    /// </summary>
    private void ShowAutoStart()
    {
        var enabled = _autoStart.IsEnabled();
        AutoStartMenuItem.IsChecked = enabled;
        _trayIcon.ShowAutoStart(enabled);
    }

    /// <summary>
    /// クリックスルーを切り替え、メニューの表示と設定ファイルに反映する
    /// </summary>
    private void SetClickThrough(bool enabled)
    {
        NativeWindow.SetClickThrough(new WindowInteropHelper(this).Handle, enabled);

        _clickThrough = enabled;
        ClickThroughMenuItem.IsChecked = enabled;
        _trayIcon.ShowClickThrough(enabled);

        if ((_settings.ClickThrough ?? false) != enabled)
        {
            _settings.ClickThrough = enabled;
            _settingsStore.Save(_settings);
        }
    }

    /// <summary>
    /// 設定画面を開く。すでに開いているときは、その画面を手前に出す
    /// </summary>
    private void OpenSettings()
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        // Owner にすると、最前面の時計よりも手前に表示され、時計を終了したときに一緒に閉じる
        _settingsWindow = new SettingsWindow(TimeText.FontSize, BackgroundBrush.Opacity) { Owner = this };
        _settingsWindow.FontSizeChanged += ApplyFontSize;
        _settingsWindow.BackgroundOpacityChanged += ApplyBackgroundOpacity;
        _settingsWindow.Closed += (_, _) =>
        {
            _settingsWindow = null;
            SaveAppearance();
        };
        _settingsWindow.Show();
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 文字サイズを反映し、背景が数字にぴったり合うよう、行の枠と数字の間のすき間を詰める
    /// </summary>
    private void ApplyFontSize(double fontSize)
    {
        TimeText.FontSize = fontSize;

        var typeface = new Typeface(TimeText.FontFamily, TimeText.FontStyle, TimeText.FontWeight, TimeText.FontStretch);
        var insets = DigitInsets.Measure(typeface, fontSize, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        // 負の余白で、すき間の分だけ背景を内側に寄せる
        TimeText.Margin = new Thickness(-insets.Left, -insets.Top, -insets.Right, -insets.Bottom);
    }

    private void ApplyBackgroundOpacity(double opacity)
    {
        BackgroundBrush.Opacity = opacity;
    }

    /// <summary>
    /// 現在の文字サイズと背景の不透明度を保存する。どちらも変わっていないときは何もしない
    /// </summary>
    private void SaveAppearance()
    {
        // 未保存のまま既定値で表示しているときは、既定値を設定ファイルに書き込まない
        if (ClockAppearance.NormalizeFontSize(_settings.FontSize) == TimeText.FontSize
            && ClockAppearance.NormalizeBackgroundOpacity(_settings.BackgroundOpacity) == BackgroundBrush.Opacity)
        {
            return;
        }

        _settings.FontSize = TimeText.FontSize;
        _settings.BackgroundOpacity = BackgroundBrush.Opacity;
        _settingsStore.Save(_settings);
    }

    /// <summary>
    /// 保存した位置にウィンドウを移動する。保存した位置がない・画面外のときは、XAML で指定した位置のままにする
    /// </summary>
    private void RestorePosition()
    {
        if (_settings.Left is not double left || _settings.Top is not double top)
        {
            return;
        }

        var screen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        if (WindowPlacement.IsOnScreen(new Rect(left, top, ActualWidth, ActualHeight), screen))
        {
            Left = left;
            Top = top;
        }
    }

    /// <summary>
    /// 現在の位置を保存する。位置が変わっていないときは何もしない
    /// </summary>
    private void SavePosition()
    {
        if (_settings.Left == Left && _settings.Top == Top)
        {
            return;
        }

        _settings.Left = Left;
        _settings.Top = Top;
        _settingsStore.Save(_settings);
    }

    /// <summary>
    /// 現在時刻を「時:分」で表示する。表示が変わらないときは何もしない
    /// </summary>
    private void UpdateTime()
    {
        var text = TimeFormatter.Format(DateTime.Now);
        if (TimeText.Text != text)
        {
            TimeText.Text = text;
        }
    }
}
