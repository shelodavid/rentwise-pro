using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models.Admin;

namespace RentWisePro.Web.Services;

public class EtlOpsMetricsService
{
    private static readonly string CompletedStatus = "Completed";
    private static readonly string FailedStatus = "Failed";
    private static readonly string QueuedStatus = "queued";
    private static readonly string ProcessingStatus = "processing";
    private static readonly string FailedQueueStatus = "failed";

    private readonly EtlReadDbContext _dbContext;

    public EtlOpsMetricsService(EtlReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EtlOpsIndexVm> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var last24hStart = now.AddHours(-24);
        var last7dStart = now.AddDays(-7);

        var last24h = await GetRunWindowAsync(last24hStart, cancellationToken);
        var last7d = await GetRunWindowAsync(last7dStart, cancellationToken);

        var recentRuns = await _dbContext.EtlRuns
            .AsNoTracking()
            .OrderByDescending(run => run.StartedAt)
            .Take(20)
            .Select(run => new EtlRecentRunVm
            {
                RunId = run.RunId,
                StartedAt = run.StartedAt,
                FinishedAt = run.FinishedAt,
                DurationMs = run.FinishedAt.HasValue
                    ? (long?)EF.Functions.DateDiffMillisecond(run.StartedAt, run.FinishedAt.Value)
                    : null,
                Status = run.Status,
                Notes = run.Notes
            })
            .ToListAsync(cancellationToken);

        var lastRunPerSource = await _dbContext.EtlRunSourceStats
            .AsNoTracking()
            .Join(
                _dbContext.EtlRuns.AsNoTracking(),
                stat => stat.RunId,
                run => run.RunId,
                (stat, run) => new { stat, run })
            .GroupBy(item => item.stat.Source)
            .Select(group => group
                .OrderByDescending(item => item.run.StartedAt)
                .Select(item => new EtlSourceRunStatsVm
                {
                    Source = item.stat.Source,
                    StartedAt = item.run.StartedAt,
                    FetchedCount = item.stat.ListingsFetched,
                    UpsertedCount = item.stat.ListingsUpserted,
                    SnapshotCount = item.stat.SnapshotsCreated,
                    MissingCount = _dbContext.EtlListings
                        .AsNoTracking()
                        .Count(listing =>
                            listing.Source == item.stat.Source &&
                            listing.LastSeenAt < item.run.StartedAt),
                    Errors = item.stat.Errors,
                    DurationMs = item.stat.DurationMs
                })
                .FirstOrDefault())
            .ToListAsync(cancellationToken);

        var last24hStats = await _dbContext.EtlRunSourceStats
            .AsNoTracking()
            .Join(
                _dbContext.EtlRuns.AsNoTracking(),
                stat => stat.RunId,
                run => run.RunId,
                (stat, run) => new { stat, run })
            .Where(item => item.run.StartedAt >= last24hStart)
            .GroupBy(item => item.stat.Source)
            .Select(group => new EtlSourceWindowStatsVm
            {
                Source = group.Key,
                FetchedCount = group.Sum(item => item.stat.ListingsFetched),
                UpsertedCount = group.Sum(item => item.stat.ListingsUpserted),
                SnapshotCount = group.Sum(item => item.stat.SnapshotsCreated),
                MissingCount = _dbContext.EtlListings
                    .AsNoTracking()
                    .Count(listing =>
                        listing.Source == group.Key &&
                        listing.LastSeenAt < last24hStart),
                Errors = group.Sum(item => item.stat.Errors),
                AverageDurationMs = group.Average(item => (double?)item.stat.DurationMs)
            })
            .ToListAsync(cancellationToken);

        var summaryBySource = new Dictionary<string, EtlSourceSummaryVm>(StringComparer.OrdinalIgnoreCase);
        foreach (var lastRun in lastRunPerSource.Where(item => item is not null))
        {
            if (lastRun is null)
            {
                continue;
            }

            summaryBySource[lastRun.Source] = new EtlSourceSummaryVm
            {
                Source = lastRun.Source,
                LastRun = lastRun,
                Last24Hours = new EtlSourceWindowStatsVm { Source = lastRun.Source }
            };
        }

        foreach (var windowStats in last24hStats)
        {
            if (!summaryBySource.TryGetValue(windowStats.Source, out var summary))
            {
                summary = new EtlSourceSummaryVm { Source = windowStats.Source };
                summaryBySource[windowStats.Source] = summary;
            }

            summary.Last24Hours = windowStats;
        }

        var workQueue = await _dbContext.EtlWorkQueueItems
            .AsNoTracking()
            .GroupBy(item => 1)
            .Select(group => new
            {
                Queued = group.Count(item => item.Status == QueuedStatus),
                Processing = group.Count(item => item.Status == ProcessingStatus),
                Failed = group.Count(item => item.Status == FailedQueueStatus),
                OldestQueuedAt = group
                    .Where(item => item.Status == QueuedStatus)
                    .Min(item => (DateTimeOffset?)item.AvailableAt)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var workQueueVm = new EtlWorkQueueHealthVm
        {
            QueuedCount = workQueue?.Queued ?? 0,
            ProcessingCount = workQueue?.Processing ?? 0,
            FailedCount = workQueue?.Failed ?? 0,
            OldestQueuedAge = workQueue?.OldestQueuedAt.HasValue == true
                ? now - workQueue.OldestQueuedAt.Value
                : null
        };

        var summaries = summaryBySource.Values
            .OrderBy(summary => summary.Source)
            .ToList();

        return new EtlOpsIndexVm
        {
            Last24Hours = last24h,
            Last7Days = last7d,
            SourceSummaries = summaries,
            WorkQueue = workQueueVm,
            RecentRuns = recentRuns
        };
    }

    public async Task<EtlRunDetailsVm?> GetRunDetailsAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await _dbContext.EtlRuns
            .AsNoTracking()
            .Where(item => item.RunId == runId)
            .Select(item => new EtlRecentRunVm
            {
                RunId = item.RunId,
                StartedAt = item.StartedAt,
                FinishedAt = item.FinishedAt,
                DurationMs = item.FinishedAt.HasValue
                    ? (long?)EF.Functions.DateDiffMillisecond(item.StartedAt, item.FinishedAt.Value)
                    : null,
                Status = item.Status,
                Notes = item.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (run is null)
        {
            return null;
        }

        var sourceStats = await _dbContext.EtlRunSourceStats
            .AsNoTracking()
            .Where(stat => stat.RunId == runId)
            .OrderBy(stat => stat.Source)
            .Select(stat => new EtlSourceRunStatsVm
            {
                Source = stat.Source,
                StartedAt = run.StartedAt,
                FetchedCount = stat.ListingsFetched,
                UpsertedCount = stat.ListingsUpserted,
                SnapshotCount = stat.SnapshotsCreated,
                MissingCount = _dbContext.EtlListings
                    .AsNoTracking()
                    .Count(listing =>
                        listing.Source == stat.Source &&
                        listing.LastSeenAt < run.StartedAt),
                Errors = stat.Errors,
                DurationMs = stat.DurationMs
            })
            .ToListAsync(cancellationToken);

        return new EtlRunDetailsVm
        {
            Run = run,
            SourceStats = sourceStats
        };
    }

    private async Task<EtlRunWindowVm> GetRunWindowAsync(DateTimeOffset windowStart, CancellationToken cancellationToken)
    {
        var windowQuery = _dbContext.EtlRuns
            .AsNoTracking()
            .Where(run => run.StartedAt >= windowStart);

        var total = await windowQuery.CountAsync(cancellationToken);
        var completed = await windowQuery.CountAsync(run => run.Status == CompletedStatus, cancellationToken);
        var failed = await windowQuery.CountAsync(run => run.Status == FailedStatus, cancellationToken);
        var averageDuration = await windowQuery
            .Where(run => run.FinishedAt.HasValue)
            .AverageAsync(run => (double?)EF.Functions.DateDiffMillisecond(run.StartedAt, run.FinishedAt ?? run.StartedAt), cancellationToken);

        return new EtlRunWindowVm
        {
            TotalRuns = total,
            CompletedRuns = completed,
            FailedRuns = failed,
            AverageDurationMs = averageDuration
        };
    }
}
