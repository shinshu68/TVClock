using System.IO;
using System.Text.Json;

namespace TVClock;

/// <summary>
/// 設定を JSON ファイルに保存し、読み込む
/// </summary>
public class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;

    // 保存済みの設定を読めないまま保存すると、読めなかった設定を空の設定で上書きして消してしまう
    private bool _loadFailed;

    /// <param name="path">設定ファイルのパス</param>
    public SettingsStore(string path)
    {
        _path = path;
    }

    /// <summary>
    /// ユーザーごとの領域にある設定ファイルのパス (%APPDATA%\TVClock\settings.json)
    /// </summary>
    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TVClock",
        "settings.json");

    /// <summary>
    /// 設定を読み込む。ファイルがない・壊れている・読めないときは、何も保存されていない設定を返す
    /// </summary>
    /// <remarks>
    /// ファイルがあるのに読めなかったとき (ほかのプロセスが開いている、権限がないなど) は、以後の <see cref="Save"/> で上書きしない
    /// </remarks>
    public AppSettings Load()
    {
        try
        {
            var json = File.ReadAllText(_path);
            _loadFailed = false;
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or JsonException)
        {
            _loadFailed = false;
            return new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _loadFailed = true;
            return new AppSettings();
        }
    }

    /// <summary>
    /// 設定を保存する。保存できたら true を返す。保存済みの設定を読めなかったときは、上書きせずに false を返す
    /// </summary>
    public bool Save(AppSettings settings)
    {
        if (_loadFailed)
        {
            return false;
        }

        var tempPath = _path + ".tmp";
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 直接上書きすると、書き込みの途中で終了したときにファイルが壊れるため、書き終えた一時ファイルで置き換える
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, settings, JsonOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(tempPath, _path, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 保存できなくても時計の表示は続ける
            return false;
        }
    }
}
