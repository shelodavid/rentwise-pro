namespace RentWisePro.Etl.Options;

public class EtlExecutionOptions
{
    public bool RunOnce { get; set; }
    public bool QueueOnly { get; set; }
    public bool QueueRunOnce { get; set; }
    public string? SourceFilter { get; set; }
    public DateTimeOffset? Since { get; set; }
    public int PageSize { get; set; } = 50;
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(12);
}
