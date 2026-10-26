using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace TVClock;

/// <summary>
/// 文字の輪郭の外側を縁取って描く。TextBlock には文字の縁取りがないため、文字を図形にして描く
/// </summary>
public class OutlinedText : FrameworkElement
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(OutlinedText),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill),
        typeof(Brush),
        typeof(OutlinedText),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke),
        typeof(Brush),
        typeof(OutlinedText),
        new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OutlineWidthProperty = DependencyProperty.Register(
        nameof(OutlineWidth),
        typeof(double),
        typeof(OutlinedText),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedText));
    public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(OutlinedText));
    public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(OutlinedText));
    public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(OutlinedText));
    public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(OutlinedText));

    /// <summary>
    /// 表示する文字列
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// 文字の中を塗る色
    /// </summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// 縁の色
    /// </summary>
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// 文字の輪郭から外側への縁の幅。0 のときは縁取りしない。要素の大きさは、文字の行の枠より上下左右に縁の幅の分だけ大きくなる
    /// </summary>
    public double OutlineWidth
    {
        get => (double)GetValue(OutlineWidthProperty);
        set => SetValue(OutlineWidthProperty, value);
    }

    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStretch FontStretch
    {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    /// <summary>
    /// 表示に使うフォント
    /// </summary>
    public Typeface Typeface => new(FontFamily, FontStyle, FontWeight, FontStretch);

    protected override Size MeasureOverride(Size availableSize)
    {
        var formatted = CreateFormattedText();
        return new Size(
            formatted.WidthIncludingTrailingWhitespace + (OutlineWidth * 2),
            formatted.Height + (OutlineWidth * 2));
    }

    // 文字の形と大きさは拡大率ごとに組み直すが、TextBlock と違い、拡大率が変わっても自動では測り直さない
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var geometry = CreateFormattedText().BuildGeometry(new Point(OutlineWidth, OutlineWidth));

        if (OutlineWidth > 0)
        {
            // 線は輪郭の内と外に半分ずつかかるため、2 倍の太さで描いてから中を塗り、外側だけを残す
            var pen = new Pen(Stroke, OutlineWidth * 2) { LineJoin = PenLineJoin.Round };
            drawingContext.DrawGeometry(null, pen, geometry);
        }

        drawingContext.DrawGeometry(Fill, null, geometry);
    }

    private FormattedText CreateFormattedText() =>
        new(
            Text,
            // 数字のすき間を測る DigitInsets と同じカルチャにしないと、数字の字形が変わる言語で余白がずれる
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface,
            FontSize,
            Fill,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
}
