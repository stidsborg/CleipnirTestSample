namespace CleipnirTestSample.MessageDriven;

public record Order(string OrderId, Guid CustomerId, IEnumerable<Guid> ProductIds, decimal TotalPrice);