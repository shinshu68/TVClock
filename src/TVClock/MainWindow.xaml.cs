using System.Windows;
using System.Windows.Input;
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

    public MainWindow()
    {
        InitializeComponent();

        _settings = _settingsStore.Load();

        _timer = new DispatcherTimer { Interval = CheckInterval };
        _timer.Tick += (_, _) => UpdateTime();

        UpdateTime();
        _timer.Start();

        // ウィンドウの大きさは内容に合わせて決まるため、大きさが確定してから位置を復元する
        Loaded += (_, _) => RestorePosition();
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
