namespace ConsumerDotnet.Services;

public interface IEventStore
{
    void Add(object eventRecord);
    IReadOnlyList<object> GetAll();
    void Clear();
    int Count { get; }
}
