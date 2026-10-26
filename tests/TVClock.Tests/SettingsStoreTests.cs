namespace TVClock.Tests;

public class SettingsStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "TVClock.Tests", Guid.NewGuid().ToString());

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    private SettingsStore CreateStoreWithoutWait() =>
        new(SettingsPath, retryCount: 1, retryDelay: TimeSpan.Zero);

    private void WriteSettingsFile(string content)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, content);
    }

    // ほかのプロセスが設定ファイルを開いている状態の代わり
    private FileStream OpenExclusively() =>
        new(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.None);

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Load_保存した位置を読み込める()
    {
        var store = new SettingsStore(SettingsPath);

        var saved = store.Save(new AppSettings { Left = 123.5, Top = -456 });
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.True(saved);
        Assert.Equal(123.5, settings.Left);
        Assert.Equal(-456, settings.Top);
    }

    [Fact]
    public void Load_保存した文字サイズと背景の不透明度を読み込める()
    {
        var store = new SettingsStore(SettingsPath);

        store.Save(new AppSettings { Left = 1, Top = 2, FontSize = 120, BackgroundOpacity = 0.3 });
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Equal(1, settings.Left);
        Assert.Equal(2, settings.Top);
        Assert.Equal(120, settings.FontSize);
        Assert.Equal(0.3, settings.BackgroundOpacity);
    }

    [Fact]
    public void Load_保存した縁の太さを読み込める()
    {
        var store = new SettingsStore(SettingsPath);

        store.Save(new AppSettings { OutlineWidth = 3 });
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Equal(3, settings.OutlineWidth);
    }

    [Fact]
    public void Load_位置だけが保存されたファイルでは文字サイズと背景の不透明度が未保存になる()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, "{ \"Left\": 10, \"Top\": 20 }");

        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Equal(10, settings.Left);
        Assert.Equal(20, settings.Top);
        Assert.Null(settings.FontSize);
        Assert.Null(settings.BackgroundOpacity);
        Assert.Null(settings.ClickThrough);
        Assert.Null(settings.OutlineWidth);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Load_保存したクリックスルーの状態を読み込める(bool clickThrough)
    {
        var store = new SettingsStore(SettingsPath);

        store.Save(new AppSettings { ClickThrough = clickThrough });
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Equal(clickThrough, settings.ClickThrough);
    }

    [Fact]
    public void Load_ファイルがないときは位置が未保存の設定を返す()
    {
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Null(settings.Left);
        Assert.Null(settings.Top);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{ \"Left\": 10, ")]
    [InlineData("{ \"Left\": \"abc\", \"Top\": 20 }")]
    [InlineData("[1, 2]")]
    public void Load_ファイルが壊れているときは位置が未保存の設定を返す(string content)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, content);

        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Null(settings.Left);
        Assert.Null(settings.Top);
    }

    [Fact]
    public void Save_壊れたファイルを読み込んだあとは上書きして保存できる()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, "{ \"Left\": 10, ");
        var store = new SettingsStore(SettingsPath);
        store.Load();

        var saved = store.Save(new AppSettings { Left = 30, Top = 40 });

        Assert.True(saved);
        Assert.Equal(30, new SettingsStore(SettingsPath).Load().Left);
    }

    [Fact]
    public async Task Load_ほかのプロセスが開いている間はやり直し閉じたら保存済みの設定を返す()
    {
        WriteSettingsFile("{ \"Left\": 10, \"Top\": 20 }");
        var store = new SettingsStore(SettingsPath, retryCount: 50, retryDelay: TimeSpan.FromMilliseconds(20));

        var locked = OpenExclusively();
        var loading = Task.Run(store.Load);
        await Task.Delay(100);
        locked.Dispose();
        var loaded = await loading;

        Assert.Equal(10, loaded.Left);
        Assert.Equal(20, loaded.Top);
    }

    [Fact]
    public void Load_やり直しても読めないときは位置が未保存の設定を返す()
    {
        WriteSettingsFile("{ \"Left\": 10, \"Top\": 20 }");

        AppSettings loaded;
        using (OpenExclusively())
        {
            loaded = CreateStoreWithoutWait().Load();
        }

        Assert.Null(loaded.Left);
        Assert.Null(loaded.Top);
    }

    [Fact]
    public void Save_読み込めなかったあとも読めないままなら保存済みの設定を上書きしない()
    {
        var original = "{ \"Left\": 10, \"Top\": 20 }";
        WriteSettingsFile(original);
        var store = CreateStoreWithoutWait();

        bool saved;
        using (OpenExclusively())
        {
            store.Load();
            saved = store.Save(new AppSettings { Left = 30, Top = 40 });
        }

        Assert.False(saved);
        Assert.Equal(original, File.ReadAllText(SettingsPath));
    }

    [Fact]
    public void Save_読み込めなかったあとに読めるようになったら変えた項目だけを保存済みの設定に上書きする()
    {
        WriteSettingsFile("{ \"Left\": 10, \"Top\": 20, \"FontSize\": 120, \"ClickThrough\": true }");
        var store = CreateStoreWithoutWait();
        using (OpenExclusively())
        {
            store.Load();
        }

        var saved = store.Save(new AppSettings { Left = 30, Top = 40 });
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.True(saved);
        Assert.Equal(30, settings.Left);
        Assert.Equal(40, settings.Top);
        Assert.Equal(120, settings.FontSize);
        Assert.True(settings.ClickThrough);
    }

    [Fact]
    public void Save_読み直したあとの保存でも保存済みのほかの項目を消さない()
    {
        WriteSettingsFile("{ \"Left\": 10, \"Top\": 20, \"FontSize\": 120, \"ClickThrough\": true }");
        var store = CreateStoreWithoutWait();
        using (OpenExclusively())
        {
            store.Load();
        }

        var current = new AppSettings { Left = 30, Top = 40 };
        store.Save(current);
        current.BackgroundOpacity = 0.5;
        store.Save(current);
        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Equal(30, settings.Left);
        Assert.Equal(120, settings.FontSize);
        Assert.Equal(0.5, settings.BackgroundOpacity);
        Assert.True(settings.ClickThrough);
    }

    [Fact]
    public void Save_読み込めなかったあとに設定ファイルが消えていたら変えた項目を保存する()
    {
        WriteSettingsFile("{ \"Left\": 10, \"Top\": 20 }");
        var store = CreateStoreWithoutWait();
        using (OpenExclusively())
        {
            store.Load();
        }
        File.Delete(SettingsPath);

        var saved = store.Save(new AppSettings { Left = 30, Top = 40 });

        Assert.True(saved);
        Assert.Equal(30, new SettingsStore(SettingsPath).Load().Left);
    }

    [Fact]
    public void Load_知らない項目があっても位置を読み込める()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, "{ \"Left\": 10, \"Top\": 20, \"Unknown\": true }");

        var settings = new SettingsStore(SettingsPath).Load();

        Assert.Equal(10, settings.Left);
        Assert.Equal(20, settings.Top);
    }

    [Fact]
    public void Save_保存先のフォルダがなければ作る()
    {
        var path = Path.Combine(_directory, "nested", "settings.json");

        var saved = new SettingsStore(path).Save(new AppSettings { Left = 1, Top = 2 });

        Assert.True(saved);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Save_保存済みの内容を上書きする()
    {
        var store = new SettingsStore(SettingsPath);
        store.Save(new AppSettings { Left = 1, Top = 2 });

        store.Save(new AppSettings { Left = 30, Top = 40 });
        var settings = store.Load();

        Assert.Equal(30, settings.Left);
        Assert.Equal(40, settings.Top);
    }

    [Fact]
    public void Save_保存できないときは例外を出さずにfalseを返す()
    {
        // 保存先と同じ名前のフォルダがあると、ファイルとして書き込めない
        Directory.CreateDirectory(SettingsPath);

        var saved = new SettingsStore(SettingsPath).Save(new AppSettings { Left = 1, Top = 2 });

        Assert.False(saved);
    }

    [Fact]
    public void Save_書き込みに失敗しても保存済みの設定を残す()
    {
        var store = new SettingsStore(SettingsPath);
        store.Save(new AppSettings { Left = 1, Top = 2 });
        // 一時ファイルと同じ名前のフォルダがあると、一時ファイルに書き込めない
        Directory.CreateDirectory(SettingsPath + ".tmp");

        var saved = store.Save(new AppSettings { Left = 30, Top = 40 });

        Assert.False(saved);
        Assert.Equal(1, new SettingsStore(SettingsPath).Load().Left);
    }

    [Fact]
    public void Save_置き換えに失敗しても保存済みの設定と一時ファイルを残さない()
    {
        var original = "{ \"Left\": 10, \"Top\": 20 }";
        WriteSettingsFile(original);

        bool saved;
        using (OpenExclusively())
        {
            saved = new SettingsStore(SettingsPath).Save(new AppSettings { Left = 30, Top = 40 });
        }

        Assert.False(saved);
        Assert.Equal(original, File.ReadAllText(SettingsPath));
        Assert.Equal([SettingsPath], Directory.GetFiles(_directory));
    }

    [Fact]
    public void Save_保存したあとに一時ファイルを残さない()
    {
        new SettingsStore(SettingsPath).Save(new AppSettings { Left = 1, Top = 2 });

        Assert.Equal([SettingsPath], Directory.GetFiles(_directory));
    }
}
