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

    /// <summary>
    /// 文字サイズ。保存されていないときは null
    /// </summary>
    public double? FontSize { get; set; }

    /// <summary>
    /// 背景の不透明度 (0〜1)。保存されていないときは null
    /// </summary>
    public double? BackgroundOpacity { get; set; }

    /// <summary>
    /// クリックスルー (時計の上のマウス操作を下のウィンドウへ通す状態) にしているか。保存されていないときは null
    /// </summary>
    public bool? ClickThrough { get; set; }
}
