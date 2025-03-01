namespace CleipnirTestSample.Rpc;

public static class TestExtensions
{
    public static Task<T> ToTask<T>(this T t) => Task.FromResult(t);
}

public static class Busy
{
    public static async Task Wait(Func<bool> predicate)
    {
        while (!predicate())
            await Task.Delay(100);
    }
}