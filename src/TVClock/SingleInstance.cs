using System.Threading;

namespace TVClock;

/// <summary>
/// 同じユーザーのセッションでアプリが 1 つだけ動くように、名前付きの Mutex で起動中かどうかを判定する
/// </summary>
public sealed class SingleInstance : IDisposable
{
    /// <summary>
    /// 既定の Mutex の名前。Local\ を付けて、サインイン中のセッションごとに 1 つとする
    /// </summary>
    public const string DefaultName = @"Local\TVClock.SingleInstance";

    private readonly Mutex _mutex;

    private SingleInstance(Mutex mutex)
    {
        _mutex = mutex;
    }

    /// <summary>
    /// ほかに起動しているものがなければ、起動中の印を取得して返す。すでに起動中なら null を返す
    /// </summary>
    /// <param name="name">Mutex の名前</param>
    public static SingleInstance? TryAcquire(string name)
    {
        try
        {
            // 所有権は使わず、Mutex が存在するかどうかだけで判定する。
            // 所有権で判定すると、取得したスレッドから解放しなければならず、終了時の扱いが面倒になるため
            var mutex = new Mutex(initiallyOwned: false, name, out var createdNew);
            if (!createdNew)
            {
                mutex.Dispose();
                return null;
            }

            return new SingleInstance(mutex);
        }
        catch (UnauthorizedAccessException)
        {
            // 管理者として起動した時計が作った Mutex は開けないが、起動中であることに変わりはない
            return null;
        }
    }

    public void Dispose()
    {
        _mutex.Dispose();
    }
}
