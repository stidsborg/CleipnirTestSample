using Cleipnir.Flows;
using Cleipnir.ResilientFunctions.Storage;
using CleipnirTestSample.Rpc.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace CleipnirTestSample.Rpc;

[TestClass]
public class OrderFlowTests
{
    [TestMethod]
    public async Task TheSameTransactionIdIsUsedAfterACrash()
    {
        var usedTransactionIds = new List<TransactionId>(); 
        
        var emailClient = new EmailClientStub(sendOrderConfirmationCallback: (_, _) => Task.CompletedTask);
        var logisticsClient = new LogisticsClientStub(shipProductsCallback: (_, _) => new TrackAndTraceNumber("1234").ToTask());
        var paymentProviderClient = new PaymentProviderClientStub(
            reserveCallback: (_, transactionId, _) => { usedTransactionIds.Add(transactionId); return Task.CompletedTask; },
            captureCallback: _ => Task.CompletedTask,
            cancelReservationCallback: _ => Task.CompletedTask
        );
        var orderFlows = CreateOrderFlows(emailClient, logisticsClient, paymentProviderClient);

        var order = new Order(
            "SomeOrderId",
            new CustomerId(Guid.NewGuid()),
            ProductIds: [new ProductId(Guid.NewGuid())],
            TotalPrice: 120.5M
        );
        await orderFlows.Run("SomeInstance", order);

        var controlPanel = await orderFlows.ControlPanel("SomeInstance");
        await controlPanel!.Restart();

        Assert.AreEqual(2, usedTransactionIds.Count);
        Assert.AreEqual(1, usedTransactionIds.Distinct().Count());
    }
    
    [TestMethod]
    public async Task ShipProductsIsCalledAtMostOnce()
    {
        var shipProductsCalls = 0; 
        
        var emailClient = new EmailClientStub(sendOrderConfirmationCallback: (_, _) => Task.CompletedTask);
        var logisticsClient = new LogisticsClientStub(shipProductsCallback: (_, _) =>
        {
            shipProductsCalls++;
            return new TrackAndTraceNumber("1234").ToTask();
        });
        var paymentProviderClient = new PaymentProviderClientStub(
            reserveCallback: (_, _, _) => Task.CompletedTask,
            captureCallback: _ => Task.CompletedTask,
            cancelReservationCallback: _ => Task.CompletedTask
        );
        var orderFlows = CreateOrderFlows(emailClient, logisticsClient, paymentProviderClient);

        var order = new Order(
            "SomeOrderId",
            new CustomerId(Guid.NewGuid()),
            ProductIds: [new ProductId(Guid.NewGuid())],
            TotalPrice: 120.5M
        );
        await orderFlows.Run("SomeInstance", order);

        var controlPanel = await orderFlows.ControlPanel("SomeInstance");
        await controlPanel!.Restart();

        Assert.AreEqual(1, shipProductsCalls);
    }

    private OrderFlows CreateOrderFlows(
        IEmailClient emailClient,
        ILogisticsClient logisticsClient,
        IPaymentProviderClient paymentProviderClient
        )
    {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<OrderFlow>();
            serviceCollection.AddSingleton(emailClient);
            serviceCollection.AddSingleton(logisticsClient);
            serviceCollection.AddSingleton(paymentProviderClient);
        
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var container = new FlowsContainer(new InMemoryFunctionStore(), serviceProvider, Options.Default);
            var orderFlows = new OrderFlows(container);
            return orderFlows;
    }
}