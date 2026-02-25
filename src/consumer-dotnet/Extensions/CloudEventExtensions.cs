using System.Text.Json;
using CloudNative.CloudEvents;

namespace ConsumerDotnet.Extensions;

public static class CloudEventExtensions
{
    public static T GetData<T>(this CloudEvent cloudEvent)
    {
        return cloudEvent.Data switch
        {
            T typed => typed,
            JsonElement je => je.Deserialize<T>()!,
            _ => throw new InvalidOperationException(
                $"Cannot deserialize CloudEvent data to {typeof(T).Name}")
        };
    }
}
