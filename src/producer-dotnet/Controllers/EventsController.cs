using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using ProducerDotnet.Services;

namespace ProducerDotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ICloudEventPublisher _publisher;

    public EventsController(ICloudEventPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("order-created")]
    public async Task<IActionResult> OrderCreated()
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

        var results = await _publisher.PublishAsync(cloudEvent);
        return Ok(new { eventId = cloudEvent.Id, type = cloudEvent.Type, sentTo = results });
    }

    [HttpPost("order-shipped")]
    public async Task<IActionResult> OrderShipped()
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

        var results = await _publisher.PublishAsync(cloudEvent);
        return Ok(new { eventId = cloudEvent.Id, type = cloudEvent.Type, sentTo = results });
    }

    [HttpPost("batch")]
    public async Task<IActionResult> Batch()
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

            var results = await _publisher.PublishAsync(cloudEvent);
            events.Add(new { eventId = cloudEvent.Id, type = cloudEvent.Type, sentTo = results });
        }

        return Ok(new { batchSize = events.Count, events });
    }
}
