using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;

namespace ProducerDotnet.Services;

public class CloudEventPublisher : ICloudEventPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudEventPublisher> _logger;
    private readonly JsonEventFormatter _formatter = new();
    private readonly string[] _consumerUrls;

    public CloudEventPublisher(HttpClient httpClient, ILogger<CloudEventPublisher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _consumerUrls =
        [
            Environment.GetEnvironmentVariable("CONSUMER_DOTNET_URL") ?? "http://localhost:5002",
            Environment.GetEnvironmentVariable("CONSUMER_NODE_URL") ?? "http://localhost:3002",
            Environment.GetEnvironmentVariable("CONSUMER_PYTHON_URL") ?? "http://localhost:8000"
        ];
    }

    public async Task<List<PublishResult>> PublishAsync(CloudEvent cloudEvent)
    {
        var results = new List<PublishResult>();

        foreach (var url in _consumerUrls)
        {
            try
            {
                var content = cloudEvent.ToHttpContent(ContentMode.Structured, _formatter);
                var response = await _httpClient.PostAsync($"{url}/api/events", content);

                results.Add(new PublishResult(url, (int)response.StatusCode, response.IsSuccessStatusCode));
            }
            catch (Exception ex)
            {
                results.Add(new PublishResult(url, 0, false, ex.Message));
                _logger.LogWarning("Failed to send CloudEvent to {Consumer}: {Error}", url, ex.Message);
            }
        }

        return results;
    }
}
