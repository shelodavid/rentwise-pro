using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RentWisePro.Etl.Core.Options;

namespace RentWisePro.Etl.Sources.Clients;

public class RapidApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RapidApiClient> _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public RapidApiClient(HttpClient httpClient, ILogger<RapidApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response => !response.IsSuccessStatusCode)
            })
            .Build();
    }

    public async Task<JsonDocument?> GetJsonAsync(Uri requestUri, string apiKey, string apiHost, CancellationToken cancellationToken)
    {
        var response = await _pipeline.ExecuteAsync(
            async token =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Add("X-RapidAPI-Key", apiKey);
                request.Headers.Add("X-RapidAPI-Host", apiHost);
                return await _httpClient.SendAsync(request, token);
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("RapidAPI request failed with status {StatusCode}", response.StatusCode);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }
}
