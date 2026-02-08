namespace RentWisePro.Etl.Core.Entities;

public class Property
{
    public Guid PropertyId { get; set; }
    public string NormalizedAddress { get; set; } = string.Empty;
    public string NormalizedAddressHash { get; set; } = string.Empty;
    public string? OriginalAddress { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PropertyType { get; set; }
    public int? YearBuilt { get; set; }
    public int? SquareFeet { get; set; }
    public decimal? Beds { get; set; }
    public decimal? Baths { get; set; }
    public decimal? EstimatedMonthlyRent { get; set; }
    public string? RentEstimateSource { get; set; }
    public DateTimeOffset? RentEstimateAsOf { get; set; }
    public int NormalizationVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
}
