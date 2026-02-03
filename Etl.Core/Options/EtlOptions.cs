namespace RentWisePro.Etl.Core.Options;

public class EtlOptions
{
    public bool DevMode { get; set; }
    public int MarkOffMarketAfterMissingRuns { get; set; } = 3;
    public int MaxPhotosPerProperty { get; set; } = 10;
    public int PhotoDownloadConcurrency { get; set; } = 4;
}
