using Cleipnir.Flows;
using Cleipnir.ResilientFunctions.Domain;
using CleipnirTestSample.Rpc.Clients;

namespace CleipnirTestSample.Rpc;

[GenerateFlows]
public class OrderFlow(
    IPaymentProviderClient paymentProviderClient,
    IEmailClient emailClient,
    ILogisticsClient logisticsClient
) : Flow<Order>
{
    public override async Task Run(Order order)
    {
        var transactionId = await Capture(TransactionId.New);
        await paymentProviderClient.Reserve(order.CustomerId, transactionId, order.TotalPrice);

        var traceNumber = await Capture(
            () => logisticsClient.ShipProducts(order.CustomerId, order.ProductIds),
            ResiliencyLevel.AtMostOnce
        );
        
        await paymentProviderClient.Capture(transactionId);
        await emailClient.SendOrderConfirmation(order, traceNumber);
    }
}