using CloudNative.CloudEvents;

namespace ProducerDotnet.Services;

public interface ICloudEventPublisher
{
    Task<List<PublishResult>> PublishAsync(CloudEvent cloudEvent);
}

public record PublishResult(string Consumer, int Status, bool Success, string? Error = null);
