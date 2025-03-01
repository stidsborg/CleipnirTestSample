namespace CleipnirTestSample.Rpc.Clients;

public record TrackAndTraceNumber(string Value);

public interface ILogisticsClient
{
    Task<TrackAndTraceNumber> ShipProducts(CustomerId customerId, IEnumerable<ProductId> productIds);
}

public class LogisticsClientStub(Func<CustomerId, IEnumerable<ProductId>, Task<TrackAndTraceNumber>> shipProductsCallback) : ILogisticsClient
{
    public Task<TrackAndTraceNumber> ShipProducts(CustomerId customerId, IEnumerable<ProductId> productIds)
        => shipProductsCallback(customerId, productIds);
}