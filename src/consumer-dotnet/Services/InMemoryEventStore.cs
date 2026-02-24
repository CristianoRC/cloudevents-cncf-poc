using System.Collections.Concurrent;

namespace ConsumerDotnet.Services;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentBag<object> _events = [];

    public void Add(object eventRecord) => _events.Add(eventRecord);

    public IReadOnlyList<object> GetAll() => [.. _events];

    public void Clear() => _events.Clear();

    public int Count => _events.Count;
}
