using Microsoft.Win32;

namespace TVClock.Tests;

public class AutoStartTests : IDisposable
{
    private const string ValueName = "TVClock";
    private const string ExecutablePath = @"C:\Program Files\TVClock\TVClock.exe";

    // 実際の自動起動の登録先には触れず、テスト専用のキーを使う
    private const string TestRootKeyPath = @"Software\TVClock.Tests";

    private readonly string _keyPath = $@"{TestRootKeyPath}\{Guid.NewGuid()}";

    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(_keyPath, throwOnMissingSubKey: false);

        // テスト専用のキーをレジストリに残さない
        using var root = Registry.CurrentUser.OpenSubKey(TestRootKeyPath);
        if (root is { SubKeyCount: 0, ValueCount: 0 })
        {
            Registry.CurrentUser.DeleteSubKey(TestRootKeyPath, throwOnMissingSubKey: false);
        }
    }

    private AutoStart Create(string executablePath = ExecutablePath) => new(_keyPath, ValueName, executablePath);

    private object? ReadValue()
    {
        using var key = Registry.CurrentUser.OpenSubKey(_keyPath);
        return key?.GetValue(ValueName);
    }

    [Fact]
    public void IsEnabled_登録していないときはfalseを返す()
    {
        Assert.False(Create().IsEnabled());
    }

    [Fact]
    public void SetEnabled_有効にするとexeのパスを引用符で囲んで登録する()
    {
        var autoStart = Create();

        var succeeded = autoStart.SetEnabled(true);

        Assert.True(succeeded);
        Assert.True(autoStart.IsEnabled());
        Assert.Equal($"\"{ExecutablePath}\"", ReadValue());
    }

    [Fact]
    public void SetEnabled_無効にすると登録が消える()
    {
        var autoStart = Create();
        autoStart.SetEnabled(true);

        var succeeded = autoStart.SetEnabled(false);

        Assert.True(succeeded);
        Assert.False(autoStart.IsEnabled());
        Assert.Null(ReadValue());
    }

    [Fact]
    public void SetEnabled_登録していない状態で無効にしても成功する()
    {
        Assert.True(Create().SetEnabled(false));
    }

    [Fact]
    public void SetEnabled_もう一度有効にすると今のexeのパスで上書きする()
    {
        Create(@"C:\Old\TVClock.exe").SetEnabled(true);

        Create().SetEnabled(true);

        Assert.Equal($"\"{ExecutablePath}\"", ReadValue());
    }

    [Fact]
    public void SetEnabled_exeのパスが分からないときは登録せずfalseを返す()
    {
        var autoStart = Create(executablePath: "");

        Assert.False(autoStart.SetEnabled(true));
        Assert.False(autoStart.IsEnabled());
    }

    [Fact]
    public void SetEnabled_同じキーのほかの値は消さない()
    {
        using (var key = Registry.CurrentUser.CreateSubKey(_keyPath))
        {
            key.SetValue("OtherApp", "other.exe");
        }

        var autoStart = Create();
        autoStart.SetEnabled(true);
        autoStart.SetEnabled(false);

        using var opened = Registry.CurrentUser.OpenSubKey(_keyPath);
        Assert.Equal("other.exe", opened?.GetValue("OtherApp"));
    }
}
