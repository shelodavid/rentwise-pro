namespace RentWisePro.Etl.Core.Interfaces;

public interface IGeoMarketDataLookup
{
    Task<decimal?> GetHudFmrAsync(string? zip, int bedrooms, int year, CancellationToken cancellationToken);
    Task<decimal?> GetVacancyRateAsync(string? zip, int year, CancellationToken cancellationToken);
    Task<decimal?> GetMedianIncomeAsync(string? zip, int year, CancellationToken cancellationToken);
}
