namespace RentWisePro.Etl.Core.Models;

public class SourceListing
{
    public string SourceListingId { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Price { get; set; }
    public decimal? Beds { get; set; }
    public decimal? Baths { get; set; }
    public int? SquareFeet { get; set; }
    public string? Status { get; set; }
    public string? PropertyType { get; set; }
    public int? YearBuilt { get; set; }
    public decimal? LotSize { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public string RawJson { get; set; } = string.Empty;
}
