using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Options;
using RentWisePro.Etl.Options;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Services;

public class RentForecastService
{
    private const string StubSource = "rentometer_stub";
    private readonly EtlDbContext _dbContext;
    private readonly EtlOptions _etlOptions;
    private readonly RentometerOptions _rentometerOptions;
    private readonly ILogger<RentForecastService> _logger;

    public RentForecastService(
        EtlDbContext dbContext,
        IOptions<EtlOptions> etlOptions,
        IOptions<RentometerOptions> rentometerOptions,
        ILogger<RentForecastService> logger)
    {
        _dbContext = dbContext;
        _etlOptions = etlOptions.Value;
        _rentometerOptions = rentometerOptions.Value;
        _logger = logger;
    }

    public async Task<RentForecastResult> ProcessAsync(WorkQueueItem item, CancellationToken cancellationToken)
    {
        if (_etlOptions.DevMode)
        {
            await UpsertStubForecastAsync(item, cancellationToken);
            return RentForecastResult.Completed("Stub rent forecast created in dev mode.");
        }

        if (string.IsNullOrWhiteSpace(_rentometerOptions.ApiKey))
        {
            return RentForecastResult.Failed(
                "Rentometer API key is missing. Set Rentometer:ApiKey or enable Etl:DevMode for stub forecasts.",
                retry: false);
        }

        _logger.LogWarning(
            "Rent forecast integration is not implemented. Work item {WorkId} cannot be completed.",
            item.WorkId);

        return RentForecastResult.Failed(
            "Rentometer integration is not implemented in this build.",
            retry: false);
    }

    private async Task UpsertStubForecastAsync(WorkQueueItem item, CancellationToken cancellationToken)
    {
        var source = StubSource;
        var existing = await _dbContext.RentForecasts.FirstOrDefaultAsync(
            forecast => forecast.PropertyId == item.PropertyId &&
                        forecast.ListingId == item.ListingId &&
                        forecast.Source == source,
            cancellationToken);

        var estimatedRent = BuildDeterministicRent(item.ListingId ?? item.PropertyId);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            existing = new RentForecast
            {
                ForecastId = Guid.NewGuid(),
                PropertyId = item.PropertyId,
                ListingId = item.ListingId,
                Source = source,
                EstimatedRent = estimatedRent,
                IsStub = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            _dbContext.RentForecasts.Add(existing);
        }
        else
        {
            existing.EstimatedRent = estimatedRent;
            existing.IsStub = true;
            existing.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static decimal BuildDeterministicRent(Guid seed)
    {
        var hash = SHA256.HashData(seed.ToByteArray());
        var value = Math.Abs(BitConverter.ToInt32(hash, 0));
        var offset = value % 1000;
        return 1200m + offset;
    }

    public sealed record RentForecastResult(bool Success, bool Retry, string? Message)
    {
        public static RentForecastResult Completed(string message) => new(true, false, message);
        public static RentForecastResult Failed(string message, bool retry) => new(false, retry, message);
    }
}
