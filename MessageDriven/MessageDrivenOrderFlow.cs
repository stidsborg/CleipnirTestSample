using Cleipnir.Flows;

namespace CleipnirTestSample.MessageDriven;

[GenerateFlows]
public class MessageDrivenOrderFlow(IBus bus) : Flow<Order>
{
    public override async Task Run(Order order)
    {
        var transactionId = await Capture(Guid.NewGuid);

        await ReserveFunds(order, transactionId);
        await Message<FundsReserved>();
        
        await ShipProducts(order);
        var productsShipped = await Message<ProductsShipped>();
        var trackAndTraceNumber = productsShipped.TrackAndTraceNumber;
        
        await CaptureFunds(order, transactionId);
        await Message<FundsCaptured>();
        
        await SendOrderConfirmationEmail(order, trackAndTraceNumber);
        await Message<OrderConfirmationEmailSent>();
    }
    
    private Task ReserveFunds(Order order, Guid transactionId) 
        => Capture(async () => await bus.SendLocal(new ReserveFunds(order.OrderId, order.TotalPrice, transactionId, order.CustomerId)));
    private Task ShipProducts(Order order)
        => Capture(async () => await bus.SendLocal(new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds)));
    private Task CaptureFunds(Order order, Guid transactionId)
        => Capture(async () => await bus.SendLocal(new CaptureFunds(order.OrderId, order.CustomerId, transactionId)));
    private Task SendOrderConfirmationEmail(Order order, string trackAndTraceNumber)
        => Capture(async () => await bus.SendLocal(new SendOrderConfirmationEmail(order.OrderId, order.CustomerId, trackAndTraceNumber)));
    private Task CancelProductsShipment(Order order)
        => Capture(async () => await bus.SendLocal(new CancelProductsShipment(order.OrderId)));
    private Task CancelFundsReservation(Order order, Guid transactionId)
        => Capture(async () => await bus.SendLocal(new CancelFundsReservation(order.OrderId, transactionId)));
    private Task ReversePayment(Order order, Guid transactionId)
        => Capture(async () => await bus.SendLocal(new ReverseTransaction(order.OrderId, transactionId)));
    
}
