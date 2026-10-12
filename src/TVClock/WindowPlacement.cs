using System.Windows;

namespace TVClock;

/// <summary>
/// ウィンドウの位置が画面に収まっているかを判定する
/// </summary>
public static class WindowPlacement
{
    // ドラッグでつかめるよう、最低限これだけは画面内に見えている必要がある
    private const double MinVisibleSize = 40;

    /// <summary>
    /// ウィンドウが画面内に、つかめる大きさで見えているかを返す
    /// </summary>
    /// <param name="window">ウィンドウの位置と大きさ</param>
    /// <param name="screen">全モニターを合わせた領域（仮想スクリーン）</param>
    public static bool IsOnScreen(Rect window, Rect screen)
    {
        if (!double.IsFinite(window.Left) || !double.IsFinite(window.Top))
        {
            return false;
        }

        var visible = Rect.Intersect(window, screen);
        if (visible.IsEmpty)
        {
            return false;
        }

        // ウィンドウ自体が小さいときは、全体が見えていればよい
        return visible.Width >= Math.Min(MinVisibleSize, window.Width)
            && visible.Height >= Math.Min(MinVisibleSize, window.Height);
    }
}
