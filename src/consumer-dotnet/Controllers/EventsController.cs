using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.SystemTextJson;
using ConsumerDotnet.Models;
using ConsumerDotnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConsumerDotnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private static readonly string[] CloudEventCoreAttributes =
        ["id", "type", "source", "time", "datacontenttype", "specversion"];

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

        var data = cloudEvent.Data is JsonElement jsonElement
            ? jsonElement
            : JsonSerializer.Deserialize<JsonElement>(cloudEvent.Data?.ToString() ?? "{}");

        var extensions = cloudEvent.GetPopulatedAttributes()
            .Where(a => !CloudEventCoreAttributes.Contains(a.Key.Name))
            .ToDictionary(a => a.Key.Name, a => a.Value?.ToString());

        var eventRecord = new ReceivedCloudEvent(
            Id: cloudEvent.Id ?? Guid.NewGuid().ToString(),
            Type: cloudEvent.Type ?? "unknown",
            Source: cloudEvent.Source?.ToString(),
            Time: cloudEvent.Time,
            DataContentType: cloudEvent.DataContentType,
            Data: data,
            SpecVersion: cloudEvent.SpecVersion.VersionId,
            ReceivedAt: DateTime.UtcNow,
            Extensions: extensions
        );

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
            eventRecord.Id, eventRecord.Type, eventRecord.Source, eventRecord.Time, eventRecord.SpecVersion);

        return Ok(new EventReceivedResponse(
            Received: true,
            Consumer: "consumer-dotnet",
            EventId: eventRecord.Id,
            EventType: eventRecord.Type
        ));
    }

    [HttpGet]
    public IActionResult List()
    {
        return Ok(new EventListResponse(
            Consumer: "consumer-dotnet",
            TotalReceived: _eventStore.Count,
            Events: _eventStore.GetAll()
        ));
    }

    [HttpDelete]
    public IActionResult Clear()
    {
        _eventStore.Clear();
        return Ok(new { cleared = true });
    }
}
