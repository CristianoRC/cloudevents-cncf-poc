using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var formatter = new JsonEventFormatter();
var consumerUrls = new[]
{
    Environment.GetEnvironmentVariable("CONSUMER_DOTNET_URL") ?? "http://localhost:5002",
    Environment.GetEnvironmentVariable("CONSUMER_NODE_URL") ?? "http://localhost:3002"
};

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "producer-dotnet" }));

app.MapPost("/api/events/order-created", async (HttpContext context) =>
{
    var orderId = Guid.NewGuid().ToString();
    var cloudEvent = new CloudEvent
    {
        Id = Guid.NewGuid().ToString(),
        Type = "com.example.order.created",
        Source = new Uri("/producer-dotnet/orders", UriKind.Relative),
        Time = DateTimeOffset.UtcNow,
        DataContentType = MediaTypeNames.Application.Json,
        Data = JsonSerializer.Serialize(new
        {
            orderId,
            customerId = $"customer-{Random.Shared.Next(1000, 9999)}",
            items = new[]
            {
                new { productId = "PROD-001", name = "Notebook Dell", quantity = 1, price = 4599.90m },
                new { productId = "PROD-002", name = "Mouse Logitech", quantity = 2, price = 149.90m }
            },
            total = 4899.70m,
            currency = "BRL",
            createdAt = DateTime.UtcNow
        }),
        ["partitionkey"] = orderId
    };

    var results = await SendToConsumers(cloudEvent);
    return Results.Ok(new { eventId = cloudEvent.Id, type = cloudEvent.Type, sentTo = results });
});

app.MapPost("/api/events/order-shipped", async (HttpContext context) =>
{
    var orderId = Guid.NewGuid().ToString();
    var cloudEvent = new CloudEvent
    {
        Id = Guid.NewGuid().ToString(),
        Type = "com.example.order.shipped",
        Source = new Uri("/producer-dotnet/orders", UriKind.Relative),
        Time = DateTimeOffset.UtcNow,
        DataContentType = MediaTypeNames.Application.Json,
        Data = JsonSerializer.Serialize(new
        {
            orderId,
            trackingCode = $"BR{Random.Shared.Next(100000000, 999999999)}",
            carrier = "Correios",
            estimatedDelivery = DateTime.UtcNow.AddDays(5),
            shippedAt = DateTime.UtcNow
        }),
        ["partitionkey"] = orderId
    };

    var results = await SendToConsumers(cloudEvent);
    return Results.Ok(new { eventId = cloudEvent.Id, type = cloudEvent.Type, sentTo = results });
});

app.MapPost("/api/events/batch", async (HttpContext context) =>
{
    var events = new List<object>();

    for (var i = 0; i < 5; i++)
    {
        var orderId = Guid.NewGuid().ToString();
        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "com.example.order.created",
            Source = new Uri("/producer-dotnet/orders", UriKind.Relative),
            Time = DateTimeOffset.UtcNow,
            DataContentType = MediaTypeNames.Application.Json,
            Data = JsonSerializer.Serialize(new
            {
                orderId,
                customerId = $"customer-{Random.Shared.Next(1000, 9999)}",
                total = Math.Round(Random.Shared.NextDouble() * 10000, 2),
                currency = "BRL",
                createdAt = DateTime.UtcNow
            }),
            ["partitionkey"] = orderId
        };

        var results = await SendToConsumers(cloudEvent);
        events.Add(new { eventId = cloudEvent.Id, type = cloudEvent.Type, sentTo = results });
    }

    return Results.Ok(new { batchSize = events.Count, events });
});

async Task<List<object>> SendToConsumers(CloudEvent cloudEvent)
{
    var results = new List<object>();
    using var httpClient = new HttpClient();

    foreach (var url in consumerUrls)
    {
        try
        {
            var content = cloudEvent.ToHttpContent(ContentMode.Structured, formatter);
            var response = await httpClient.PostAsync($"{url}/api/events", content);

            results.Add(new
            {
                consumer = url,
                status = (int)response.StatusCode,
                success = response.IsSuccessStatusCode
            });

            app.Logger.LogInformation(
                "CloudEvent {EventId} ({Type}) sent to {Consumer} -> {StatusCode}",
                cloudEvent.Id, cloudEvent.Type, url, response.StatusCode);
        }
        catch (Exception ex)
        {
            results.Add(new { consumer = url, status = 0, success = false, error = ex.Message });
            app.Logger.LogWarning("Failed to send CloudEvent to {Consumer}: {Error}", url, ex.Message);
        }
    }

    return results;
}

app.Run();
