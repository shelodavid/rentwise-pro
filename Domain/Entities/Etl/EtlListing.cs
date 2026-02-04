namespace RentWisePro.Web.Domain.Entities.Etl;

public class EtlListing
{
    public Guid ListingId { get; set; }
    public Guid PropertyId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string SourceListingId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }

    public EtlProperty? Property { get; set; }
}
