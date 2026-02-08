namespace RentWisePro.Web.Services.MarketData
{
    public class NullGeoMarketDataLookup : IGeoMarketDataLookup
    {
        public Task<IReadOnlyDictionary<int, GeoMarketMetrics>> GetMetricsAsync(
            IReadOnlyCollection<RentalListingMarketKey> listings,
            CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<int, GeoMarketMetrics> empty = new Dictionary<int, GeoMarketMetrics>();
            return Task.FromResult(empty);
        }
    }
}
