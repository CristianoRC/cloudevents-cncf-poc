using ProducerDotnet.Services;

namespace ProducerDotnet.Models;

public record EventResponse(
    string EventId,
    string Type,
    List<PublishResult> SentTo
);
