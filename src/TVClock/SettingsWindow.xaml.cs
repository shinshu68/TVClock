using System.Windows;

namespace TVClock;

/// <summary>
/// 文字サイズと背景の不透明度を変更する設定画面。変更はその場で時計に反映する
/// </summary>
public partial class SettingsWindow : Window
{
    /// <param name="fontSize">現在の文字サイズ</param>
    /// <param name="backgroundOpacity">現在の背景の不透明度（0〜1）</param>
    public SettingsWindow(double fontSize, double backgroundOpacity)
    {
        InitializeComponent();

        FontSizeSlider.Minimum = ClockAppearance.MinFontSize;
        FontSizeSlider.Maximum = ClockAppearance.MaxFontSize;
        FontSizeSlider.TickFrequency = ClockAppearance.FontSizeStep;
        FontSizeSlider.SmallChange = ClockAppearance.FontSizeStep;
        FontSizeSlider.LargeChange = ClockAppearance.FontSizeStep;
        FontSizeSlider.Value = fontSize;

        // 小数の刻みでは誤差が出るため、スライダーは百分率の整数で動かす
        BackgroundOpacitySlider.Minimum = ToPercent(ClockAppearance.MinBackgroundOpacity);
        BackgroundOpacitySlider.Maximum = ToPercent(ClockAppearance.MaxBackgroundOpacity);
        BackgroundOpacitySlider.TickFrequency = ToPercent(ClockAppearance.BackgroundOpacityStep);
        BackgroundOpacitySlider.SmallChange = ToPercent(ClockAppearance.BackgroundOpacityStep);
        BackgroundOpacitySlider.LargeChange = ToPercent(ClockAppearance.BackgroundOpacityStep);
        BackgroundOpacitySlider.Value = ToPercent(backgroundOpacity);

        UpdateTexts();

        // 初期値を入れるときに変更の通知が出ないよう、値を入れてから登録する
        FontSizeSlider.ValueChanged += (_, _) =>
        {
            UpdateTexts();
            FontSizeChanged?.Invoke(FontSizeSlider.Value);
        };
        BackgroundOpacitySlider.ValueChanged += (_, _) =>
        {
            UpdateTexts();
            BackgroundOpacityChanged?.Invoke(BackgroundOpacitySlider.Value / 100);
        };
    }

    /// <summary>
    /// 文字サイズが変更されたときに、新しい文字サイズを通知する
    /// </summary>
    public event Action<double>? FontSizeChanged;

    /// <summary>
    /// 背景の不透明度が変更されたときに、新しい不透明度（0〜1）を通知する
    /// </summary>
    public event Action<double>? BackgroundOpacityChanged;

    private static double ToPercent(double ratio) => Math.Round(ratio * 100);

    private void UpdateTexts()
    {
        FontSizeText.Text = FontSizeSlider.Value.ToString("0");
        BackgroundOpacityText.Text = $"{BackgroundOpacitySlider.Value:0}%";
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
