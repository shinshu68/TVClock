using System.Globalization;

namespace TVClock;

/// <summary>
/// 時刻を画面に表示する文字列に変換する
/// </summary>
public static class TimeFormatter
{
    /// <summary>
    /// 時刻を 24 時間表記の「時:分」（例：09:05）にする
    /// </summary>
    public static string Format(DateTime time)
    {
        // OS の地域設定で区切り文字や数字が変わらないよう、カルチャを固定する
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
