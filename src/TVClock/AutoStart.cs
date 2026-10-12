using System.IO;
using System.Security;
using Microsoft.Win32;

namespace TVClock;

/// <summary>
/// Windows の起動時（サインイン時）にアプリを自動で起動するかどうかを、ユーザーごとのレジストリに登録する
/// </summary>
public class AutoStart
{
    /// <summary>
    /// サインイン時に起動するアプリを登録するキー（HKEY_CURRENT_USER の下）
    /// </summary>
    public const string DefaultKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public const string DefaultValueName = "TVClock";

    private readonly string _keyPath;
    private readonly string _valueName;
    private readonly string _executablePath;

    /// <param name="keyPath">登録先のキー（HKEY_CURRENT_USER からのパス）</param>
    /// <param name="valueName">登録する値の名前</param>
    /// <param name="executablePath">起動する exe のパス</param>
    public AutoStart(string keyPath, string valueName, string executablePath)
    {
        _keyPath = keyPath;
        _valueName = valueName;
        _executablePath = executablePath;
    }

    /// <summary>
    /// 自動起動が登録されているかを返す。レジストリを読めないときは false を返す
    /// </summary>
    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(_keyPath);
            return key?.GetValue(_valueName) is string command && command.Length > 0;
        }
        catch (Exception ex) when (IsRegistryError(ex))
        {
            return false;
        }
    }

    /// <summary>
    /// 自動起動を登録する、または登録を消す。できたら true を返す
    /// </summary>
    public bool SetEnabled(bool enabled)
    {
        try
        {
            if (enabled)
            {
                if (string.IsNullOrEmpty(_executablePath))
                {
                    return false;
                }

                using var key = Registry.CurrentUser.CreateSubKey(_keyPath);

                // パスに空白があっても 1 つのコマンドとして扱われるよう、引用符で囲む
                key.SetValue(_valueName, $"\"{_executablePath}\"");
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(_keyPath, writable: true);
                key?.DeleteValue(_valueName, throwOnMissingValue: false);
            }

            return true;
        }
        catch (Exception ex) when (IsRegistryError(ex))
        {
            // 登録できなくても時計の表示は続ける
            return false;
        }
    }

    private static bool IsRegistryError(Exception ex) =>
        ex is SecurityException or UnauthorizedAccessException or IOException;
}
