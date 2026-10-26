namespace TVClock;

/// <summary>
/// 時計の見た目 (文字サイズ・背景の不透明度・縁の太さ) の既定値と範囲を持ち、設定の値を表示に使える値に直す
/// </summary>
public static class ClockAppearance
{
    public const double DefaultFontSize = 96;
    public const double MinFontSize = 24;
    public const double MaxFontSize = 300;
    public const double FontSizeStep = 1;

    /// <summary>
    /// 設定画面でスライダーの溝をクリックしたときなど、大きく動かすときの幅
    /// </summary>
    public const double FontSizeLargeStep = 12;

    // 背景の不透明度は 0 (背景なし) 〜 1 (透けない) で表す
    public const double DefaultBackgroundOpacity = 0;
    public const double MinBackgroundOpacity = 0;
    public const double MaxBackgroundOpacity = 1;
    public const double BackgroundOpacityStep = 0.1;

    // 縁の太さは文字の輪郭から外側への幅 (ピクセル) で、0 は縁取りなし
    public const double DefaultOutlineWidth = 2;
    public const double MinOutlineWidth = 0;
    public const double MaxOutlineWidth = 5;
    public const double OutlineWidthStep = 1;

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

    /// <summary>
    /// 設定の縁の太さを、表示に使える値に直す。未保存・数値として不正なときは既定値を返し、範囲外のときは範囲内に丸める
    /// </summary>
    public static double NormalizeOutlineWidth(double? value) =>
        Normalize(value, DefaultOutlineWidth, MinOutlineWidth, MaxOutlineWidth);

    /// <summary>
    /// 表示中の見た目のうち、設定の値と違う項目だけを設定に書き込む。書き込んだ項目があれば true を返す
    /// </summary>
    /// <remarks>
    /// 変えていない項目まで書き込むと、設定を読めずに既定値で表示しているときに、保存済みの値を既定値で上書きしてしまう
    /// </remarks>
    public static bool UpdateSettings(AppSettings settings, double fontSize, double backgroundOpacity, double outlineWidth)
    {
        var changed = false;

        if (NormalizeFontSize(settings.FontSize) != fontSize)
        {
            settings.FontSize = fontSize;
            changed = true;
        }

        if (NormalizeBackgroundOpacity(settings.BackgroundOpacity) != backgroundOpacity)
        {
            settings.BackgroundOpacity = backgroundOpacity;
            changed = true;
        }

        if (NormalizeOutlineWidth(settings.OutlineWidth) != outlineWidth)
        {
            settings.OutlineWidth = outlineWidth;
            changed = true;
        }

        return changed;
    }

    private static double Normalize(double? value, double defaultValue, double min, double max)
    {
        if (value is not double number || !double.IsFinite(number))
        {
            return defaultValue;
        }

        return Math.Clamp(number, min, max);
    }
}
