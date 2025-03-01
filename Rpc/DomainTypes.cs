namespace CleipnirTestSample.Rpc;

public record Order(string OrderId, CustomerId CustomerId, IEnumerable<ProductId> ProductIds, decimal TotalPrice);
public record CustomerId(Guid Value);
public record ProductId(Guid Value);

public record TransactionId(Guid Value)
{
    public static TransactionId New() => new(Guid.NewGuid());
}