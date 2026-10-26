using System.Windows;

namespace TVClock;

/// <summary>
/// アプリ全体の起動と終了を扱う
/// </summary>
public partial class App : Application
{
    private SingleInstance? _singleInstance;

    /// <summary>
    /// すでに時計が起動しているときは、何も表示せずに終了する
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstance = SingleInstance.TryAcquire(SingleInstance.DefaultName);
        if (_singleInstance is null)
        {
            Shutdown();
            return;
        }

        // 起動中かどうかを確かめてからウィンドウを作るため、StartupUri は使わずにここで開く
        new MainWindow().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}

