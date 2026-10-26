using System.Windows;
using System.Windows.Media;

namespace TVClock.Tests;

public class OutlinedTextTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    public void Measure_縁の幅の分だけ上下左右に大きくなる(double outlineWidth)
    {
        var (withoutOutline, withOutline) = RunOnStaThread(() =>
            (MeasureText(outlineWidth: 0), MeasureText(outlineWidth)));

        Assert.Equal(withoutOutline.Width + (outlineWidth * 2), withOutline.Width, 6);
        Assert.Equal(withoutOutline.Height + (outlineWidth * 2), withOutline.Height, 6);
    }

    [Fact]
    public void Measure_縁の幅を変えると大きさを測り直す()
    {
        var (before, after) = RunOnStaThread(() =>
        {
            var text = CreateText(outlineWidth: 1);
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var before = text.DesiredSize;

            text.OutlineWidth = 4;
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return (before, text.DesiredSize);
        });

        Assert.Equal(before.Width + 6, after.Width, 6);
    }

    private static Size MeasureText(double outlineWidth)
    {
        var text = CreateText(outlineWidth);
        text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return text.DesiredSize;
    }

    private static OutlinedText CreateText(double outlineWidth) =>
        new()
        {
            Text = "12:34",
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 96,
            OutlineWidth = outlineWidth,
        };

    // WPF の要素は STA のスレッドでしか作れない
    private static T RunOnStaThread<T>(Func<T> action)
    {
        T result = default!;
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            throw error;
        }

        return result;
    }
}
