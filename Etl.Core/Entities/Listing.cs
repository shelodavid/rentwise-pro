namespace RentWisePro.Etl.Core.Entities;

public class Listing
{
    public Guid ListingId { get; set; }
    public Guid PropertyId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string SourceListingId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal? EstimatedRent { get; set; }
    public decimal? RprMonthly { get; set; }
    public decimal? Grm { get; set; }
    public decimal? EstimatedCashFlow { get; set; }
    public decimal? AffordabilityIndex { get; set; }
    public decimal? PricePerSqft { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset? SoldAt { get; set; }
    public string MaterialHash { get; set; } = string.Empty;
    public int MissingRuns { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Property? Property { get; set; }
    public ICollection<ListingSnapshot> Snapshots { get; set; } = new List<ListingSnapshot>();
}
