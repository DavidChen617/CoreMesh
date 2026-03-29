using System.Collections.Concurrent;
using System.Net;
using System.Text;
using CoreMesh.Middleware.Extensions;
using CoreMesh.Middleware.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace CoreMesh.Middleware.Tests.Idempotency;

public class IdempotencyMiddlewareTests
{
    private async Task<HttpClient> CreateClient()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddCoreMeshMiddleware(middleware =>
            middleware.AddIdempotency(idempotency =>
                idempotency.WithHandler(new InMemoryTestHandler())
            )
        );

        var app = builder.Build();

        app.UseCoreMeshMiddleware();

        app.MapPost("/orders", [Idempotency] () => Results.Ok(new { orderId = Guid.NewGuid() }));
        app.MapPost("/payments", [Idempotency("X-Payment-Key")] () => Results.Ok(new { paymentId = Guid.NewGuid() }));
        app.MapPost("/no-idempotency", () => Results.Ok(new { value = Guid.NewGuid() }));
        app.MapGet("/orders", () => Results.Ok());

        await app.StartAsync();

        return app.GetTestClient();
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string url, string? idempotencyKey = null, string headerName = "Idempotency-Key")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        if (idempotencyKey is not null)
            request.Headers.Add(headerName, idempotencyKey);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task MissingIdempotencyKey_Returns400()
    {
        var client = await CreateClient();

        var response = await Post(client, "/orders");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FirstRequest_ReturnsOkWithoutReplayedHeader()
    {
        var client = await CreateClient();

        var response = await Post(client, "/orders", "test-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Idempotency-Replayed"));
    }

    [Fact]
    public async Task SecondRequestWithSameKey_ReturnsReplayedResponse()
    {
        var client = await CreateClient();

        var first = await Post(client, "/orders", "test-002");
        var firstBody = await first.Content.ReadAsStringAsync();

        var second = await Post(client, "/orders", "test-002");
        var secondBody = await second.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.True(second.Headers.Contains("X-Idempotency-Replayed"));
        Assert.Equal(firstBody, secondBody);
    }

    [Fact]
    public async Task DifferentKeys_ReturnDifferentResponses()
    {
        var client = await CreateClient();

        var first = await Post(client, "/orders", "key-A");
        var second = await Post(client, "/orders", "key-B");

        var firstBody = await first.Content.ReadAsStringAsync();
        var secondBody = await second.Content.ReadAsStringAsync();

        Assert.NotEqual(firstBody, secondBody);
    }

    [Fact]
    public async Task EndpointWithoutAttribute_NotIntercepted()
    {
        var client = await CreateClient();

        var first = await Post(client, "/no-idempotency", "test-003");
        var second = await Post(client, "/no-idempotency", "test-003");

        var firstBody = await first.Content.ReadAsStringAsync();
        var secondBody = await second.Content.ReadAsStringAsync();

        Assert.False(second.Headers.Contains("X-Idempotency-Replayed"));
        Assert.NotEqual(firstBody, secondBody);
    }

    [Fact]
    public async Task GetRequest_NotIntercepted()
    {
        var client = await CreateClient();

        var response = await client.GetAsync("/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CustomHeaderName_MissingHeader_Returns400()
    {
        var client = await CreateClient();

        var response = await Post(client, "/payments");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CustomHeaderName_SecondRequestWithSameKey_ReturnsReplayed()
    {
        var client = await CreateClient();

        await Post(client, "/payments", "pay-001", "X-Payment-Key");
        var second = await Post(client, "/payments", "pay-001", "X-Payment-Key");

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.True(second.Headers.Contains("X-Idempotency-Replayed"));
    }
}

file class InMemoryTestHandler : IIdempotencyHandler
{
    private readonly ConcurrentDictionary<string, IdempotencyResult> _store = new();

    public Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(idempotencyKey, out var result);
        return Task.FromResult(result);
    }

    public Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload, CancellationToken cancellationToken = default)
    {
        _store[idempotencyKey] = new IdempotencyResult(statusCode, responsePayload);
        return Task.CompletedTask;
    }
}
