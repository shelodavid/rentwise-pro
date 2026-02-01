using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
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
        var item = await ClaimNextAsync(dbContext, stoppingToken);
        if (item is null)
        {
            return false;
        }

        try
        {
            if (string.Equals(item.WorkType, "photo_download", StringComparison.OrdinalIgnoreCase))
            {
                await HandlePhotoDownloadAsync(photoDownloadService, item, stoppingToken);
            }
            else if (string.Equals(item.WorkType, "rent_forecast", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Rent forecast stub processed for listing {ListingId}", item.ListingId);
            }

            item.Status = "done";
            item.UpdatedAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Work item {WorkId} failed", item.WorkId);
            item.Status = "failed";
            item.UpdatedAt = DateTimeOffset.UtcNow;
            item.AvailableAt = DateTimeOffset.UtcNow.AddMinutes(Math.Min(Math.Pow(2, item.Attempts), 60));
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        return true;
    }

    private static async Task HandlePhotoDownloadAsync(
        PhotoDownloadService photoDownloadService,
        WorkQueueItem item,
        CancellationToken stoppingToken)
    {
        var payload = JsonSerializer.Deserialize<PhotoPayload>(item.PayloadJson);
        if (payload is null)
        {
            return;
        }

        await photoDownloadService.DownloadAsync(payload.PropertyId, payload.Source, payload.Photos, stoppingToken);
    }

    private async Task<WorkQueueItem?> ClaimNextAsync(EtlDbContext dbContext, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var item = await dbContext.WorkQueue
            .FromSqlRaw(@"
                SELECT TOP(1) * FROM work_queue WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE Status = 'queued' AND AvailableAt <= SYSUTCDATETIME()
                ORDER BY AvailableAt")
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        item.Status = "processing";
        item.Attempts += 1;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return item;
    }

    private sealed record PhotoPayload(Guid PropertyId, string Source, IReadOnlyList<string> Photos);
}
