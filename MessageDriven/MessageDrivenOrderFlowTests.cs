using Cleipnir.Flows;
using Cleipnir.ResilientFunctions.Storage;
using CleipnirTestSample.Rpc;
using Microsoft.Extensions.DependencyInjection;

namespace CleipnirTestSample.MessageDriven;

[TestClass]
public class MessageDrivenOrderFlowTests
{
    [TestMethod]
    public async Task CaptureFundsCommandHasSameTransactionIdAsReserveFundsCommand()
    {
        var (orderFlows, bus, disposable) = CreateOrderFlows();
        using var _ = disposable; 

        var order = new Order("SomeOrderId", CustomerId: Guid.NewGuid(), ProductIds: [Guid.NewGuid()], TotalPrice: 120.5M);
        await orderFlows.Schedule(
            instanceId: order.OrderId, 
            param: order
        );

        await orderFlows.SendMessage(flowInstance: order.OrderId, new FundsReserved(order.OrderId));
        await orderFlows.SendMessage(flowInstance: order.OrderId, new ProductsShipped(order.OrderId, "SomeTrackAndTraceNumber"));
        
        await Busy.Wait(() => bus.Messages.OfType<CaptureFunds>().Any());
        var reserveFunds = bus.Messages.OfType<ReserveFunds>().Single();
        var captureFunds = bus.Messages.OfType<CaptureFunds>().Single();
        Assert.AreEqual(reserveFunds.TransactionId, captureFunds.TransactionId);
    }
    
    private (MessageDrivenOrderFlows, BusStub, IDisposable) CreateOrderFlows()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<MessageDrivenOrderFlow>();
        var busStub = new BusStub();
        serviceCollection.AddSingleton<IBus>(busStub);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var container = new FlowsContainer(new InMemoryFunctionStore(), serviceProvider, Options.Default);
        var orderFlows = new MessageDrivenOrderFlows(container);
        return (orderFlows, busStub, container);
    }
}