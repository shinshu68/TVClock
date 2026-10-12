namespace TVClock;

/// <summary>
/// 時計の見た目（文字サイズ・背景の不透明度）の既定値と範囲を持ち、設定の値を表示に使える値に直す
/// </summary>
public static class ClockAppearance
{
    public const double DefaultFontSize = 96;
    public const double MinFontSize = 24;
    public const double MaxFontSize = 300;
    public const double FontSizeStep = 12;

    // 背景の不透明度は 0（背景なし）〜 1（透けない）で表す
    public const double DefaultBackgroundOpacity = 0.5;
    public const double MinBackgroundOpacity = 0;
    public const double MaxBackgroundOpacity = 1;
    public const double BackgroundOpacityStep = 0.1;

    /// <summary>
    /// 設定の文字サイズを、表示に使える値に直す。未保存・数値として不正なときは既定値を返し、範囲外のときは範囲内に丸める
    /// </summary>
    public static double NormalizeFontSize(double? value) =>
        Normalize(value, DefaultFontSize, MinFontSize, MaxFontSize);

    /// <summary>
    /// 設定の背景の不透明度を、表示に使える値に直す。未保存・数値として不正なときは既定値を返し、範囲外のときは範囲内に丸める
    /// </summary>
    public static double NormalizeBackgroundOpacity(double? value) =>
        Normalize(value, DefaultBackgroundOpacity, MinBackgroundOpacity, MaxBackgroundOpacity);

    private static double Normalize(double? value, double defaultValue, double min, double max)
    {
        if (value is not double number || !double.IsFinite(number))
        {
            return defaultValue;
        }

        return Math.Clamp(number, min, max);
    }
}
