using System.Collections.Concurrent;
using ConsumerDotnet.Models;

namespace ConsumerDotnet.Services;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentBag<ReceivedCloudEvent> _events = [];

    public void Add(ReceivedCloudEvent eventRecord) => _events.Add(eventRecord);

    public IReadOnlyList<ReceivedCloudEvent> GetAll() => [.. _events];

    public void Clear() => _events.Clear();

    public int Count => _events.Count;
}
