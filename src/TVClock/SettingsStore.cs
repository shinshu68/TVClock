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
    private readonly int _retryCount;
    private readonly TimeSpan _retryDelay;

    // 保存済みの設定を読めないまま保存すると、読めなかった設定を空の設定で上書きして消してしまう
    private bool _loadFailed;

    // 読み込めなかった間に変えていない項目まで空の値で保存しないよう、読み直した保存済みの設定で埋める
    private AppSettings? _savedBeforeLoadFailure;

    /// <param name="path">設定ファイルのパス</param>
    /// <param name="retryCount">読めなかったときに読み込みをやり直す回数</param>
    /// <param name="retryDelay">読み込みをやり直すまでの間隔。省略したときは 0.2 秒</param>
    public SettingsStore(string path, int retryCount = 5, TimeSpan? retryDelay = null)
    {
        _path = path;
        _retryCount = retryCount;
        _retryDelay = retryDelay ?? TimeSpan.FromMilliseconds(200);
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
    /// ファイルがあるのに読めない (ほかのプロセスが開いている、権限がないなど) ときは、間隔を空けて読み込みをやり直す。
    /// やり直しても読めなかったときは、<see cref="Save"/> のときに読み直し、読めるまでは上書きしない
    /// </remarks>
    public AppSettings Load()
    {
        _savedBeforeLoadFailure = null;
        for (var retry = 0; ; retry++)
        {
            if (TryRead(out var settings))
            {
                _loadFailed = false;
                return settings;
            }

            if (retry >= _retryCount)
            {
                _loadFailed = true;
                return new AppSettings();
            }

            Thread.Sleep(_retryDelay);
        }
    }

    /// <summary>
    /// 設定を保存する。保存できたら true を返す
    /// </summary>
    /// <remarks>
    /// <see cref="Load"/> で読めなかったときは、保存済みの設定を読み直し、<paramref name="settings"/> で値のない項目は保存済みの値のまま保存する。
    /// 読み直せないときは、上書きせずに false を返す
    /// </remarks>
    public bool Save(AppSettings settings)
    {
        if (_loadFailed)
        {
            if (!TryRead(out var saved))
            {
                return false;
            }

            _savedBeforeLoadFailure = saved;
            _loadFailed = false;
        }

        if (_savedBeforeLoadFailure is not null)
        {
            settings = FillMissing(settings, _savedBeforeLoadFailure);
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
            DeleteIfPossible(tempPath);

            // 保存できなくても時計の表示は続ける
            return false;
        }
    }

    private static void DeleteIfPossible(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 消せなかった一時ファイルは、次に保存するときに上書きされる
        }
    }

    /// <summary>
    /// 設定ファイルを読む。ファイルがない・壊れているときは、何も保存されていない設定を読めたものとして true を返す
    /// </summary>
    private bool TryRead(out AppSettings settings)
    {
        try
        {
            var json = File.ReadAllText(_path);
            settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or JsonException)
        {
            settings = new AppSettings();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            settings = new AppSettings();
            return false;
        }
    }

    /// <summary>
    /// <paramref name="settings"/> で値のない項目を、<paramref name="saved"/> の値で埋めた設定を返す
    /// </summary>
    private static AppSettings FillMissing(AppSettings settings, AppSettings saved)
    {
        var filled = new AppSettings();
        foreach (var property in typeof(AppSettings).GetProperties())
        {
            property.SetValue(filled, property.GetValue(settings) ?? property.GetValue(saved));
        }

        return filled;
    }
}
