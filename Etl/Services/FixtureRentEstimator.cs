using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Options;

namespace RentWisePro.Etl.Services;

public class FixtureRentEstimator : IRentEstimator
{
    private readonly EtlOptions _etlOptions;

    public FixtureRentEstimator(IOptions<EtlOptions> etlOptions)
    {
        _etlOptions = etlOptions.Value;
    }

    public int Priority => 100;

    public Task<RentEstimate?> EstimateAsync(Property property, CancellationToken cancellationToken)
    {
        if (!_etlOptions.DevMode)
        {
            return Task.FromResult<RentEstimate?>(null);
        }

        var estimatedRent = BuildDeterministicRent(property.PropertyId);
        var estimate = new RentEstimate(estimatedRent, "fixture", DateTimeOffset.UtcNow);
        return Task.FromResult<RentEstimate?>(estimate);
    }

    private static decimal BuildDeterministicRent(Guid seed)
    {
        var hash = SHA256.HashData(seed.ToByteArray());
        var value = Math.Abs(BitConverter.ToInt32(hash, 0));
        var offset = value % 900;
        return 1100m + offset;
    }
}
