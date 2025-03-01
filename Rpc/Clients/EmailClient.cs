namespace CleipnirTestSample.Rpc.Clients;

public interface IEmailClient
{
    Task SendOrderConfirmation(Order order, TrackAndTraceNumber trackAndTraceNumber);
}

public class EmailClientStub(Func<Order, TrackAndTraceNumber, Task> sendOrderConfirmationCallback)
    : IEmailClient
{
    public Func<Order, TrackAndTraceNumber, Task> SendOrderConfirmationCallback { get; } = sendOrderConfirmationCallback;
    public Task SendOrderConfirmation(Order order, TrackAndTraceNumber trackAndTraceNumber)
        => SendOrderConfirmationCallback(order, trackAndTraceNumber);
}