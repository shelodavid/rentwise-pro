using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Persistence.Contexts;
using RentWisePro.Etl.Services;

namespace RentWisePro.Etl.Workers;

public class WorkQueueWorker : BackgroundService
{
    private readonly EtlDbContext _dbContext;
    private readonly PhotoDownloadService _photoDownloadService;
    private readonly ILogger<WorkQueueWorker> _logger;

    public WorkQueueWorker(
        EtlDbContext dbContext,
        PhotoDownloadService photoDownloadService,
        ILogger<WorkQueueWorker> logger)
    {
        _dbContext = dbContext;
        _photoDownloadService = photoDownloadService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = await ClaimNextAsync(stoppingToken);
            if (item is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            try
            {
                if (string.Equals(item.WorkType, "photo_download", StringComparison.OrdinalIgnoreCase))
                {
                    await HandlePhotoDownloadAsync(item, stoppingToken);
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

            await _dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task HandlePhotoDownloadAsync(WorkQueueItem item, CancellationToken stoppingToken)
    {
        var payload = JsonSerializer.Deserialize<PhotoPayload>(item.PayloadJson);
        if (payload is null)
        {
            return;
        }

        await _photoDownloadService.DownloadAsync(payload.PropertyId, payload.Source, payload.Photos, stoppingToken);
    }

    private async Task<WorkQueueItem?> ClaimNextAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var item = await _dbContext.WorkQueue
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
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return item;
    }

    private sealed record PhotoPayload(Guid PropertyId, string Source, IReadOnlyList<string> Photos);
}
