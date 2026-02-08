using Microsoft.EntityFrameworkCore;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Persistence.Repositories;

public class GeoMarketDataLookup : IGeoMarketDataLookup
{
    private readonly EtlDbContext _dbContext;

    public GeoMarketDataLookup(EtlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal?> GetHudFmrAsync(string? zip, int bedrooms, int year, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(zip) || bedrooms < 0)
        {
            return null;
        }

        var query = _dbContext.HudFairMarketRents.AsNoTracking()
            .Where(row => row.GeoType == GeoTypes.Zip && row.GeoKey == zip);

        var targetYear = year > 0
            ? year
            : await query.MaxAsync(row => (int?)row.Year, cancellationToken);

        if (!targetYear.HasValue)
        {
            return null;
        }

        return await query.Where(row => row.Year == targetYear.Value && row.Bedrooms == bedrooms)
            .Select(row => (decimal?)row.Fmr)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<decimal?> GetVacancyRateAsync(string? zip, int year, CancellationToken cancellationToken)
    {
        return await GetGeoMarketStatValueAsync(zip, year, stat => stat.VacancyRate, cancellationToken);
    }

    public async Task<decimal?> GetMedianIncomeAsync(string? zip, int year, CancellationToken cancellationToken)
    {
        return await GetGeoMarketStatValueAsync(zip, year, stat => stat.MedianHouseholdIncome, cancellationToken);
    }

    private async Task<decimal?> GetGeoMarketStatValueAsync(
        string? zip,
        int year,
        Func<RentWisePro.Etl.Core.Entities.GeoMarketStat, decimal> selector,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(zip))
        {
            return null;
        }

        var query = _dbContext.GeoMarketStats.AsNoTracking()
            .Where(row => row.GeoType == GeoTypes.Zip && row.GeoKey == zip);

        var targetYear = year > 0
            ? year
            : await query.MaxAsync(row => (int?)row.Year, cancellationToken);

        if (!targetYear.HasValue)
        {
            return null;
        }

        return await query.Where(row => row.Year == targetYear.Value)
            .Select(stat => (decimal?)selector(stat))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
