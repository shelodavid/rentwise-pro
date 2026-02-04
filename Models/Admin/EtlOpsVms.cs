namespace RentWisePro.Web.Models.Admin;

public class EtlOpsIndexVm
{
    public EtlRunWindowVm Last24Hours { get; set; } = new();
    public EtlRunWindowVm Last7Days { get; set; } = new();
    public List<EtlSourceSummaryVm> SourceSummaries { get; set; } = new();
    public EtlWorkQueueHealthVm WorkQueue { get; set; } = new();
    public List<EtlRecentRunVm> RecentRuns { get; set; } = new();

    public bool HasRuns => RecentRuns.Count > 0;
}

public class EtlRunWindowVm
{
    public int TotalRuns { get; set; }
    public int CompletedRuns { get; set; }
    public int FailedRuns { get; set; }
    public double? AverageDurationMs { get; set; }
}

public class EtlSourceSummaryVm
{
    public string Source { get; set; } = string.Empty;
    public EtlSourceRunStatsVm? LastRun { get; set; }
    public EtlSourceWindowStatsVm Last24Hours { get; set; } = new();
}

public class EtlSourceRunStatsVm
{
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset? StartedAt { get; set; }
    public int FetchedCount { get; set; }
    public int UpsertedCount { get; set; }
    public int SnapshotCount { get; set; }
    public int MissingCount { get; set; }
    public int Errors { get; set; }
    public long DurationMs { get; set; }
}

public class EtlSourceWindowStatsVm
{
    public string Source { get; set; } = string.Empty;
    public int FetchedCount { get; set; }
    public int UpsertedCount { get; set; }
    public int SnapshotCount { get; set; }
    public int MissingCount { get; set; }
    public int Errors { get; set; }
    public double? AverageDurationMs { get; set; }
}

public class EtlWorkQueueHealthVm
{
    public int QueuedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int FailedCount { get; set; }
    public TimeSpan? OldestQueuedAge { get; set; }
}

public class EtlRecentRunVm
{
    public Guid RunId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public long? DurationMs { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class EtlRunDetailsVm
{
    public EtlRecentRunVm Run { get; set; } = new();
    public List<EtlSourceRunStatsVm> SourceStats { get; set; } = new();
    public bool HasSourceStats => SourceStats.Count > 0;
}
