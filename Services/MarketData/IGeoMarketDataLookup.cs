namespace RentWisePro.Web.Services.MarketData
{
    public interface IGeoMarketDataLookup
    {
        Task<IReadOnlyDictionary<int, GeoMarketMetrics>> GetMetricsAsync(
            IReadOnlyCollection<RentalListingMarketKey> listings,
            CancellationToken cancellationToken);
    }

    public record RentalListingMarketKey(
        int RentalListingId,
        string? City,
        string? State,
        string? ZipCode);

    public record GeoMarketMetrics(
        decimal? VacancyRate,
        decimal? AffordabilityIndex,
        decimal? FairMarketRent,
        decimal? MedianPricePerSqft,
        string? Source);
}
