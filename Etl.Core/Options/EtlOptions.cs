namespace RentWisePro.Etl.Core.Options;

public class EtlOptions
{
    public int MarkOffMarketAfterMissingRuns { get; set; } = 3;
    public int MaxPhotosPerProperty { get; set; } = 10;
}
