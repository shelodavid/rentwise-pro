namespace RentWisePro.Etl.Core.Entities;

public class EtlRunSourceStat
{
    public Guid RunId { get; set; }
    public string Source { get; set; } = string.Empty;
    public int ListingsFetched { get; set; }
    public int ListingsUpserted { get; set; }
    public int SnapshotsCreated { get; set; }
    public int RawPayloadsSaved { get; set; }
    public int Errors { get; set; }
    public long DurationMs { get; set; }
}
