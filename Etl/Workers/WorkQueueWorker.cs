using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Services;
using RentWisePro.Etl.Options;
using RentWisePro.Etl.Persistence.Contexts;
using RentWisePro.Etl.Services;

namespace RentWisePro.Etl.Workers;

public class WorkQueueWorker : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EtlExecutionOptions _options;
    private readonly ILogger<WorkQueueWorker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public WorkQueueWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<EtlExecutionOptions> options,
        ILogger<WorkQueueWorker> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.RunOnce && !_options.QueueOnly && !_options.QueueRunOnce)
        {
            _logger.LogInformation("Run-once ingestion requested. Skipping work queue processing.");
            return;
        }

        _logger.LogInformation(
            "Work queue processing configured (runOnce={RunOnce}, queueOnly={QueueOnly}, queueRunOnce={QueueRunOnce})",
            _options.RunOnce,
            _options.QueueOnly,
            _options.QueueRunOnce);

        if (_options.QueueRunOnce)
        {
            _logger.LogInformation("Queue drain mode enabled. Worker will exit after the queue is empty.");
        }

        using var timer = new PeriodicTimer(IdleDelay);
        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = await ProcessNextAsync(stoppingToken);
            if (!processed)
            {
                if (_options.QueueRunOnce)
                {
                    _logger.LogInformation("Queue drained. Stopping host.");
                    _applicationLifetime.StopApplication();
                    return;
                }

                await timer.WaitForNextTickAsync(stoppingToken);
                continue;
            }
        }
    }

    private async Task<bool> ProcessNextAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EtlDbContext>();
        var photoDownloadService = scope.ServiceProvider.GetRequiredService<PhotoDownloadService>();
        var rentForecastService = scope.ServiceProvider.GetRequiredService<RentForecastService>();
        var item = await ClaimNextAsync(dbContext, stoppingToken);
        if (item is null)
        {
            return false;
        }

        try
        {
            _logger.LogInformation(
                "Processing work item {WorkId} (type={WorkType}, attempts={Attempts})",
                item.WorkId,
                item.WorkType,
                item.Attempts);

            var outcome = await HandleWorkItemAsync(photoDownloadService, rentForecastService, item, stoppingToken);
            ApplyOutcome(item, outcome);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Work item {WorkId} (type={WorkType}, attempts={Attempts}) failed unexpectedly",
                item.WorkId,
                item.WorkType,
                item.Attempts);
            ApplyOutcome(item, WorkQueueOutcome.Retry($"Unhandled exception: {ex.Message}"));
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        return true;
    }

    private async Task<WorkQueueOutcome> HandleWorkItemAsync(
        PhotoDownloadService photoDownloadService,
        RentForecastService rentForecastService,
        WorkQueueItem item,
        CancellationToken stoppingToken)
    {
        if (string.Equals(item.WorkType, "photo_download", StringComparison.OrdinalIgnoreCase))
        {
            return await HandlePhotoDownloadAsync(photoDownloadService, item, stoppingToken);
        }

        if (string.Equals(item.WorkType, "rent_forecast", StringComparison.OrdinalIgnoreCase))
        {
            var result = await rentForecastService.ProcessAsync(item, stoppingToken);
            if (result.Success)
            {
                _logger.LogInformation(
                    "Rent forecast completed for work {WorkId} (listing={ListingId})",
                    item.WorkId,
                    item.ListingId);
                return WorkQueueOutcome.Done(result.Message);
            }

            return result.Retry
                ? WorkQueueOutcome.Retry(result.Message)
                : WorkQueueOutcome.Failed(result.Message);
        }

        return WorkQueueOutcome.Failed($"Unknown work type '{item.WorkType}'.");
    }

    private async Task<WorkQueueOutcome> HandlePhotoDownloadAsync(
        PhotoDownloadService photoDownloadService,
        WorkQueueItem item,
        CancellationToken stoppingToken)
    {
        var payload = JsonSerializer.Deserialize<PhotoPayload>(item.PayloadJson);
        if (payload is null)
        {
            return WorkQueueOutcome.Failed("Photo payload is missing or invalid.");
        }

        var result = await photoDownloadService.DownloadAsync(
            payload.PropertyId,
            payload.Source,
            payload.Photos,
            stoppingToken);

        if (result.Failed > 0)
        {
            var message = result.Errors.Count > 0
                ? $"Failed to download {result.Failed} of {result.Total} photos. First error: {result.Errors[0]}"
                : $"Failed to download {result.Failed} of {result.Total} photos.";
            return WorkQueueOutcome.Retry(message);
        }

        return WorkQueueOutcome.Done($"Downloaded {result.Succeeded} photos.");
    }

    private async Task<WorkQueueItem?> ClaimNextAsync(EtlDbContext dbContext, CancellationToken cancellationToken)
    {
        var item = await dbContext.WorkQueue
            .FromSqlRaw(@"
                UPDATE TOP(1) work_queue WITH (ROWLOCK, READPAST)
                SET Status = 'processing',
                    Attempts = Attempts + 1,
                    UpdatedAt = SYSUTCDATETIME()
                OUTPUT INSERTED.*
                WHERE Status = 'queued' AND AvailableAt <= SYSUTCDATETIME()
                ORDER BY AvailableAt")
            .FirstOrDefaultAsync(cancellationToken);

        return item;
    }

    private void ApplyOutcome(WorkQueueItem item, WorkQueueOutcome outcome)
    {
        var now = DateTimeOffset.UtcNow;
        if (outcome.Status == WorkQueueOutcomeStatus.Done)
        {
            item.Status = "done";
            item.UpdatedAt = now;
            _logger.LogInformation(
                "Work item {WorkId} (type={WorkType}, attempts={Attempts}) completed",
                item.WorkId,
                item.WorkType,
                item.Attempts);
            return;
        }

        item.Status = outcome.Status == WorkQueueOutcomeStatus.Retry ? "queued" : "failed";
        item.UpdatedAt = now;
        item.AvailableAt = WorkQueuePolicy.CalculateNextAvailable(now, item.Attempts);
        item.PayloadJson = UpdatePayloadWithError(item.PayloadJson, outcome.Message ?? "Work item failed.", now);

        _logger.LogWarning(
            "Work item {WorkId} (type={WorkType}, attempts={Attempts}) marked {Outcome}: {Reason}",
            item.WorkId,
            item.WorkType,
            item.Attempts,
            item.Status,
            outcome.Message);
    }

    private static string UpdatePayloadWithError(string payloadJson, string message, DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return JsonSerializer.Serialize(new { last_error = message, last_error_at = timestamp });
        }

        try
        {
            var node = JsonNode.Parse(payloadJson);
            if (node is JsonObject obj)
            {
                obj["last_error"] = message;
                obj["last_error_at"] = timestamp;
                return obj.ToJsonString();
            }
        }
        catch (JsonException)
        {
        }

        var fallback = new JsonObject
        {
            ["payload_raw"] = payloadJson,
            ["last_error"] = message,
            ["last_error_at"] = timestamp
        };
        return fallback.ToJsonString();
    }

    private sealed record PhotoPayload(Guid PropertyId, string Source, IReadOnlyList<string> Photos);

    private sealed record WorkQueueOutcome(WorkQueueOutcomeStatus Status, string? Message)
    {
        public static WorkQueueOutcome Done(string? message = null) => new(WorkQueueOutcomeStatus.Done, message);
        public static WorkQueueOutcome Retry(string? message = null) => new(WorkQueueOutcomeStatus.Retry, message);
        public static WorkQueueOutcome Failed(string? message = null) => new(WorkQueueOutcomeStatus.Failed, message);
    }

    private enum WorkQueueOutcomeStatus
    {
        Done,
        Retry,
        Failed
    }
}
