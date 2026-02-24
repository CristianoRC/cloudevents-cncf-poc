using ConsumerDotnet.Models;

namespace ConsumerDotnet.Services;

public interface IEventStore
{
    void Add(ReceivedCloudEvent eventRecord);
    IReadOnlyList<ReceivedCloudEvent> GetAll();
    void Clear();
    int Count { get; }
}
