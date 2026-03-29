using CoreMesh.Endpoints;
using CoreMesh.Middleware.Idempotency;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Examples.AspNetCore.Samples.Idempotency;


public sealed class IdempotencySampleEndpoint : IEndpoint
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("idempotency/orders",Handle);

        app.MapPost("idempotency/payments", [Idempotency("X-Payment-Key")] (
            [FromBody] CreatePaymentRequest request) =>
        {
            var paymentId = Guid.NewGuid();
            return TypedResults.Ok(new CreatePaymentResponse(paymentId, request.Amount));
        });
    }
    
    [Idempotency]
    private static IResult Handle([FromBody] CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        return TypedResults.Ok(new CreateOrderResponse(orderId, request.ProductName, request.Quantity));
    }
}

public record CreateOrderRequest(string ProductName, int Quantity);
public record CreateOrderResponse(Guid OrderId, string ProductName, int Quantity);

public record CreatePaymentRequest(decimal Amount);
public record CreatePaymentResponse(Guid PaymentId, decimal Amount);
