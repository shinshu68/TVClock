namespace TVClock;

/// <summary>
/// 設定ファイルに保存する項目
/// </summary>
public class AppSettings
{
    /// <summary>
    /// ウィンドウの左端の位置。保存されていないときは null
    /// </summary>
    public double? Left { get; set; }

    /// <summary>
    /// ウィンドウの上端の位置。保存されていないときは null
    /// </summary>
    public double? Top { get; set; }
}
