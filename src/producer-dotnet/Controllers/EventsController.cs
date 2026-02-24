using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using ProducerDotnet.Models;
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

        var data = new OrderCreatedData(
            OrderId: orderId,
            CustomerId: $"customer-{Random.Shared.Next(1000, 9999)}",
            Items:
            [
                new OrderItem("PROD-001", "Notebook Dell", 1, 4599.90m),
                new OrderItem("PROD-002", "Mouse Logitech", 2, 149.90m)
            ],
            Total: 4899.70m,
            Currency: "BRL",
            CreatedAt: DateTime.UtcNow
        );

        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "com.example.order.created",
            Source = new Uri("/producer-dotnet/orders", UriKind.Relative),
            Time = DateTimeOffset.UtcNow,
            DataContentType = MediaTypeNames.Application.Json,
            Data = JsonSerializer.Serialize(data),
            ["partitionkey"] = orderId
        };

        var results = await _publisher.PublishAsync(cloudEvent);
        return Ok(new EventResponse(cloudEvent.Id, cloudEvent.Type, results));
    }

    [HttpPost("order-shipped")]
    public async Task<IActionResult> OrderShipped()
    {
        var orderId = Guid.NewGuid().ToString();

        var data = new OrderShippedData(
            OrderId: orderId,
            TrackingCode: $"BR{Random.Shared.Next(100000000, 999999999)}",
            Carrier: "Correios",
            EstimatedDelivery: DateTime.UtcNow.AddDays(5),
            ShippedAt: DateTime.UtcNow
        );

        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "com.example.order.shipped",
            Source = new Uri("/producer-dotnet/orders", UriKind.Relative),
            Time = DateTimeOffset.UtcNow,
            DataContentType = MediaTypeNames.Application.Json,
            Data = JsonSerializer.Serialize(data),
            ["partitionkey"] = orderId
        };

        var results = await _publisher.PublishAsync(cloudEvent);
        return Ok(new EventResponse(cloudEvent.Id, cloudEvent.Type, results));
    }
}
