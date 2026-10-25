namespace TVClock.Tests;

public class SingleInstanceTests
{
    // 実際に動いている時計と衝突しないよう、テストごとに別の名前を使う
    private readonly string _name = $@"Local\TVClock.Tests.{Guid.NewGuid()}";

    [Fact]
    public void TryAcquire_ほかに起動していなければ取得できる()
    {
        using var instance = SingleInstance.TryAcquire(_name);

        Assert.NotNull(instance);
    }

    [Fact]
    public void TryAcquire_すでに取得されているときはnullを返す()
    {
        using var first = SingleInstance.TryAcquire(_name);

        using var second = SingleInstance.TryAcquire(_name);

        Assert.Null(second);
    }

    [Fact]
    public void TryAcquire_先に取得したものを破棄すると再び取得できる()
    {
        SingleInstance.TryAcquire(_name)!.Dispose();

        using var instance = SingleInstance.TryAcquire(_name);

        Assert.NotNull(instance);
    }

    [Fact]
    public void TryAcquire_取得できなかった側を破棄しても先に取得したものは残る()
    {
        using var first = SingleInstance.TryAcquire(_name);
        SingleInstance.TryAcquire(_name)?.Dispose();

        using var third = SingleInstance.TryAcquire(_name);

        Assert.Null(third);
    }
}
