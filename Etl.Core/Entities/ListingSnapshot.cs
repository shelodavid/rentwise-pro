namespace RentWisePro.Etl.Core.Entities;

public class ListingSnapshot
{
    public Guid SnapshotId { get; set; }
    public Guid ListingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string MaterialHash { get; set; } = string.Empty;
    public DateTimeOffset ScrapedAt { get; set; }
    public string? RawRef { get; set; }

    public Listing? Listing { get; set; }
}
