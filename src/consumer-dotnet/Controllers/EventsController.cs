using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.SystemTextJson;
using ConsumerDotnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConsumerDotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventsController> _logger;
    private readonly JsonEventFormatter _formatter = new();

    public EventsController(IEventStore eventStore, ILogger<EventsController> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        var cloudEvent = await Request.ToCloudEventAsync(_formatter);

        var eventRecord = new
        {
            id = cloudEvent.Id,
            type = cloudEvent.Type,
            source = cloudEvent.Source?.ToString(),
            time = cloudEvent.Time,
            dataContentType = cloudEvent.DataContentType,
            data = cloudEvent.Data is JsonElement jsonElement
                ? jsonElement
                : JsonSerializer.Deserialize<JsonElement>(cloudEvent.Data?.ToString() ?? "{}"),
            specVersion = cloudEvent.SpecVersion.VersionId,
            receivedAt = DateTime.UtcNow,
            extensions = cloudEvent.GetPopulatedAttributes()
                .Where(a => a.Key.Name is not ("id" or "type" or "source" or "time" or "datacontenttype" or "specversion"))
                .ToDictionary(a => a.Key.Name, a => a.Value?.ToString())
        };

        _eventStore.Add(eventRecord);

        _logger.LogInformation(
            """
            ╔══════════════════════════════════════════════════════════════╗
            ║  CloudEvent Received (consumer-dotnet)                      ║
            ╠══════════════════════════════════════════════════════════════╣
            ║  ID:          {EventId}
            ║  Type:        {Type}
            ║  Source:      {Source}
            ║  Time:        {Time}
            ║  SpecVersion: {SpecVersion}
            ╚══════════════════════════════════════════════════════════════╝
            """,
            cloudEvent.Id, cloudEvent.Type, cloudEvent.Source, cloudEvent.Time, cloudEvent.SpecVersion.VersionId);

        _logger.LogInformation("Data: {Data}", cloudEvent.Data?.ToString());

        return Ok(new
        {
            received = true,
            consumer = "consumer-dotnet",
            eventId = cloudEvent.Id,
            eventType = cloudEvent.Type
        });
    }

    [HttpGet]
    public IActionResult List()
    {
        return Ok(new
        {
            consumer = "consumer-dotnet",
            totalReceived = _eventStore.Count,
            events = _eventStore.GetAll()
        });
    }

    [HttpDelete]
    public IActionResult Clear()
    {
        _eventStore.Clear();
        return Ok(new { cleared = true });
    }
}
