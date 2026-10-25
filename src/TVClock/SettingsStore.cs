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
    public AppSettings Load()
    {
        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// 設定を保存する。保存できたら true を返す
    /// </summary>
    public bool Save(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_path, JsonSerializer.Serialize(settings, JsonOptions));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 保存できなくても時計の表示は続ける
            return false;
        }
    }
}
