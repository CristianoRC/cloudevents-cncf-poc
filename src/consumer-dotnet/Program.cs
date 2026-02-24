using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var formatter = new JsonEventFormatter();
var receivedEvents = new List<object>();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "consumer-dotnet" }));

app.MapPost("/api/events", async (HttpContext context) =>
{
    var cloudEvent = await context.Request.ToCloudEventAsync(formatter);

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
            .Where(a => a.Key.Name != "id" && a.Key.Name != "type" && a.Key.Name != "source"
                        && a.Key.Name != "time" && a.Key.Name != "datacontenttype" && a.Key.Name != "specversion")
            .ToDictionary(a => a.Key.Name, a => a.Value?.ToString())
    };

    receivedEvents.Add(eventRecord);

    app.Logger.LogInformation(
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

    app.Logger.LogInformation("Data: {Data}", cloudEvent.Data?.ToString());

    return Results.Ok(new
    {
        received = true,
        consumer = "consumer-dotnet",
        eventId = cloudEvent.Id,
        eventType = cloudEvent.Type
    });
});

app.MapGet("/api/events", () =>
{
    return Results.Ok(new
    {
        consumer = "consumer-dotnet",
        totalReceived = receivedEvents.Count,
        events = receivedEvents
    });
});

app.MapDelete("/api/events", () =>
{
    receivedEvents.Clear();
    return Results.Ok(new { cleared = true });
});

app.Run();
