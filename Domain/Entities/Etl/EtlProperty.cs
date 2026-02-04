namespace RentWisePro.Web.Domain.Entities.Etl;

public class EtlProperty
{
    public Guid PropertyId { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public decimal? Beds { get; set; }
    public decimal? Baths { get; set; }
    public int? SquareFeet { get; set; }

    public ICollection<EtlListing> Listings { get; set; } = new List<EtlListing>();
    public ICollection<EtlPropertyPhoto> Photos { get; set; } = new List<EtlPropertyPhoto>();
}
