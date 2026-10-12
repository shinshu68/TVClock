using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TVClock;

/// <summary>
/// 文字の行の枠と、実際に描かれる数字との間にできるすき間を測る
/// </summary>
public static class DigitInsets
{
    private const string Digits = "0123456789";

    /// <summary>
    /// 行の枠の各辺から数字までのすき間を返す。表示が欠けないよう、端数は切り捨てる
    /// </summary>
    /// <remarks>
    /// 表示中の時刻ではなく数字全体で測るため、時刻が変わっても値は変わらない。
    /// 左右は数字ごとにすき間が違うので、いちばん小さいものを返す
    /// </remarks>
    /// <param name="typeface">時刻の表示に使うフォント</param>
    /// <param name="fontSize">文字サイズ</param>
    /// <param name="pixelsPerDip">表示先の画面の拡大率</param>
    public static Thickness Measure(Typeface typeface, double fontSize, double pixelsPerDip)
    {
        // 上下は、数字をすべて並べたときに描かれる範囲で測る
        var (allBox, allInk) = MeasureText(Digits, typeface, fontSize, pixelsPerDip);
        var top = allInk.Top;
        var bottom = allBox.Height - allInk.Bottom;

        var left = double.MaxValue;
        var right = double.MaxValue;
        foreach (var digit in Digits)
        {
            var (box, ink) = MeasureText(digit.ToString(), typeface, fontSize, pixelsPerDip);
            left = Math.Min(left, ink.Left);
            right = Math.Min(right, box.Width - ink.Right);
        }

        return new Thickness(Trim(left), Trim(top), Trim(right), Trim(bottom));
    }

    /// <summary>
    /// 文字列の行の枠の大きさと、その中で実際に描かれる範囲を返す
    /// </summary>
    private static (Size Box, Rect Ink) MeasureText(string text, Typeface typeface, double fontSize, double pixelsPerDip)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            Brushes.Black,
            pixelsPerDip);

        var box = new Size(formatted.WidthIncludingTrailingWhitespace, formatted.Height);
        return (box, formatted.BuildGeometry(new Point(0, 0)).Bounds);
    }

    // 枠からはみ出して描かれるフォントでは負の値になるため、その場合は詰めない
    private static double Trim(double inset) =>
        double.IsFinite(inset) ? Math.Max(0, Math.Floor(inset)) : 0;
}
