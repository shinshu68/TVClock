using System.Windows;
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

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = CheckInterval };
        _timer.Tick += (_, _) => UpdateTime();

        UpdateTime();
        _timer.Start();
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
