using System.Text.Json;

namespace ConsumerDotnet.Models;

public record ReceivedCloudEvent(
    string Id,
    string Type,
    string? Source,
    DateTimeOffset? Time,
    string? DataContentType,
    JsonElement Data,
    string SpecVersion,
    DateTime ReceivedAt,
    Dictionary<string, string?> Extensions
);

public record EventReceivedResponse(
    bool Received,
    string Consumer,
    string EventId,
    string EventType
);

public record EventListResponse(
    string Consumer,
    int TotalReceived,
    IReadOnlyList<ReceivedCloudEvent> Events
);
