using System.Windows;

namespace TVClock;

/// <summary>
/// 文字サイズ・背景の不透明度・縁の太さを変更する設定画面。変更はその場で時計に反映する
/// </summary>
public partial class SettingsWindow : Window
{
    /// <param name="fontSize">現在の文字サイズ</param>
    /// <param name="backgroundOpacity">現在の背景の不透明度 (0〜1)</param>
    /// <param name="outlineWidth">現在の縁の太さ</param>
    public SettingsWindow(double fontSize, double backgroundOpacity, double outlineWidth)
    {
        InitializeComponent();

        FontSizeSlider.Minimum = ClockAppearance.MinFontSize;
        FontSizeSlider.Maximum = ClockAppearance.MaxFontSize;
        FontSizeSlider.TickFrequency = ClockAppearance.FontSizeStep;
        FontSizeSlider.SmallChange = ClockAppearance.FontSizeStep;
        FontSizeSlider.LargeChange = ClockAppearance.FontSizeLargeStep;
        FontSizeSlider.Value = fontSize;

        // 小数の刻みでは誤差が出るため、スライダーは百分率の整数で動かす
        BackgroundOpacitySlider.Minimum = ToPercent(ClockAppearance.MinBackgroundOpacity);
        BackgroundOpacitySlider.Maximum = ToPercent(ClockAppearance.MaxBackgroundOpacity);
        BackgroundOpacitySlider.TickFrequency = ToPercent(ClockAppearance.BackgroundOpacityStep);
        BackgroundOpacitySlider.SmallChange = ToPercent(ClockAppearance.BackgroundOpacityStep);
        BackgroundOpacitySlider.LargeChange = ToPercent(ClockAppearance.BackgroundOpacityStep);
        BackgroundOpacitySlider.Value = ToPercent(backgroundOpacity);

        OutlineWidthSlider.Minimum = ClockAppearance.MinOutlineWidth;
        OutlineWidthSlider.Maximum = ClockAppearance.MaxOutlineWidth;
        OutlineWidthSlider.TickFrequency = ClockAppearance.OutlineWidthStep;
        OutlineWidthSlider.SmallChange = ClockAppearance.OutlineWidthStep;
        OutlineWidthSlider.LargeChange = ClockAppearance.OutlineWidthStep;
        OutlineWidthSlider.Value = outlineWidth;

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
        OutlineWidthSlider.ValueChanged += (_, _) =>
        {
            UpdateTexts();
            OutlineWidthChanged?.Invoke(OutlineWidthSlider.Value);
        };
    }

    /// <summary>
    /// 文字サイズが変更されたときに、新しい文字サイズを通知する
    /// </summary>
    public event Action<double>? FontSizeChanged;

    /// <summary>
    /// 背景の不透明度が変更されたときに、新しい不透明度 (0〜1) を通知する
    /// </summary>
    public event Action<double>? BackgroundOpacityChanged;

    /// <summary>
    /// 縁の太さが変更されたときに、新しい縁の太さを通知する
    /// </summary>
    public event Action<double>? OutlineWidthChanged;

    private static double ToPercent(double ratio) => Math.Round(ratio * 100);

    private void UpdateTexts()
    {
        FontSizeText.Text = FontSizeSlider.Value.ToString("0");
        BackgroundOpacityText.Text = $"{BackgroundOpacitySlider.Value:0}%";
        OutlineWidthText.Text = $"{OutlineWidthSlider.Value:0}px";
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
