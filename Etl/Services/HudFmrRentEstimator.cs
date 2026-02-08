using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Services;

public class HudFmrRentEstimator : IRentEstimator
{
    private readonly EtlDbContext _dbContext;
    private readonly ILogger<HudFmrRentEstimator> _logger;

    public HudFmrRentEstimator(EtlDbContext dbContext, ILogger<HudFmrRentEstimator> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public int Priority => 0;

    public async Task<RentEstimate?> EstimateAsync(Property property, CancellationToken cancellationToken)
    {
        var zip = property.Zip?.Trim();
        if (string.IsNullOrWhiteSpace(zip))
        {
            return null;
        }

        if (!property.Beds.HasValue)
        {
            return null;
        }

        var roundedBedrooms = (int)Math.Round(property.Beds.Value, MidpointRounding.AwayFromZero);
        if (roundedBedrooms < 0)
        {
            roundedBedrooms = 0;
        }

        var latestYear = await _dbContext.HudFairMarketRents
            .Where(row => row.GeoCode == zip)
            .MaxAsync(row => (int?)row.Year, cancellationToken);

        if (!latestYear.HasValue)
        {
            return null;
        }

        var maxBedrooms = await _dbContext.HudFairMarketRents
            .Where(row => row.GeoCode == zip && row.Year == latestYear.Value)
            .MaxAsync(row => (int?)row.Bedrooms, cancellationToken);

        if (!maxBedrooms.HasValue)
        {
            return null;
        }

        var targetBedrooms = Math.Min(roundedBedrooms, maxBedrooms.Value);
        var row = await _dbContext.HudFairMarketRents.FirstOrDefaultAsync(
            entry => entry.GeoCode == zip && entry.Year == latestYear.Value && entry.Bedrooms == targetBedrooms,
            cancellationToken);

        if (row is null)
        {
            _logger.LogInformation("HUD FMR lookup missed for zip {Zip} bedrooms {Bedrooms}.", zip, targetBedrooms);
            return null;
        }

        return new RentEstimate(row.FmrMonthlyRent, row.Source, row.ImportedAt);
    }
}
