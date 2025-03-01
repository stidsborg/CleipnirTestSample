using System.Collections.Concurrent;

namespace CleipnirTestSample.MessageDriven;

public interface IBus
{
    Task SendLocal(object msg);
}

public class BusStub : IBus
{
    public ConcurrentBag<object> Messages { get; } = new();

    public Task SendLocal(object msg)
    {
        Messages.Add(msg);
        return Task.CompletedTask;
    }
}