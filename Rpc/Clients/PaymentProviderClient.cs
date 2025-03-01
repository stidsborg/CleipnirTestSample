namespace CleipnirTestSample.Rpc.Clients;

public interface IPaymentProviderClient
{
    Task Reserve(CustomerId customerId, TransactionId transactionId, decimal amount);
    Task Capture(TransactionId transactionId);
    Task CancelReservation(TransactionId transactionId);
}

public class PaymentProviderClientStub(
    Func<CustomerId, TransactionId, decimal, Task> reserveCallback,
    Func<TransactionId, Task> captureCallback,
    Func<TransactionId, Task> cancelReservationCallback
) : IPaymentProviderClient
{
    public Task Reserve(CustomerId customerId, TransactionId transactionId, decimal amount)
        => reserveCallback(customerId, transactionId, amount);
    public Task Capture(TransactionId transactionId) 
        => captureCallback(transactionId);
    
    public Task CancelReservation(TransactionId transactionId) 
        => cancelReservationCallback(transactionId);
}