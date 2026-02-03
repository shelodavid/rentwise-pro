namespace RentWisePro.Etl.Core.Entities;

public class RentForecast
{
    public Guid ForecastId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? ListingId { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal EstimatedRent { get; set; }
    public bool IsStub { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
