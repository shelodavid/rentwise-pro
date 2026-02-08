using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Persistence.Contexts;
using RentWisePro.Etl.Persistence.Repositories;
using RentWisePro.Etl.ReferenceData;
using Xunit;

namespace RentWisePro.Etl.Tests.ReferenceData;

public class ReferenceDataImportTests
{
    [Fact]
    public async Task HudFmrImporter_UpsertsSampleRows()
    {
        await using var context = CreateContext();
        var importer = new HudFmrReferenceImporter(context, NullLogger<HudFmrReferenceImporter>.Instance);
        var path = GetFixturePath("Hud", "hud_fmr_sample.csv");

        var first = await importer.ImportAsync(path, 2024, GeoTypes.Zip, CancellationToken.None);
        var second = await importer.ImportAsync(path, 2024, GeoTypes.Zip, CancellationToken.None);

        Assert.Equal(40, first);
        Assert.Equal(40, second);
        Assert.Equal(40, await context.HudFairMarketRents.CountAsync());
    }

    [Fact]
    public async Task GeoMarketStatsImporter_UpsertsSampleRows()
    {
        await using var context = CreateContext();
        var importer = new GeoMarketStatsImporter(context, NullLogger<GeoMarketStatsImporter>.Instance);
        var path = GetFixturePath("Acs", "geo_market_stats_sample.csv");

        var first = await importer.ImportAsync(path, 2023, GeoTypes.Zip, CancellationToken.None);
        var second = await importer.ImportAsync(path, 2023, GeoTypes.Zip, CancellationToken.None);

        Assert.Equal(10, first);
        Assert.Equal(10, second);
        Assert.Equal(10, await context.GeoMarketStats.CountAsync());
    }

    [Fact]
    public async Task GeoMarketDataLookup_ReturnsExpectedMetrics()
    {
        await using var context = CreateContext();
        context.HudFairMarketRents.Add(new HudFairMarketRent
        {
            GeoType = GeoTypes.Zip,
            GeoKey = "78701",
            Year = 2024,
            Bedrooms = 2,
            Fmr = 1800,
            RetrievedAt = DateTimeOffset.UtcNow
        });
        context.GeoMarketStats.Add(new GeoMarketStat
        {
            GeoType = GeoTypes.Zip,
            GeoKey = "78701",
            Year = 2023,
            VacancyRate = 0.045m,
            MedianHouseholdIncome = 75000,
            RetrievedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var lookup = new GeoMarketDataLookup(context);

        var fmr = await lookup.GetHudFmrAsync("78701", 2, 2024, CancellationToken.None);
        var vacancy = await lookup.GetVacancyRateAsync("78701", 2023, CancellationToken.None);
        var income = await lookup.GetMedianIncomeAsync("78701", 2023, CancellationToken.None);

        Assert.Equal(1800m, fmr);
        Assert.Equal(0.045m, vacancy);
        Assert.Equal(75000m, income);
    }

    private static EtlDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EtlDbContext(options);
    }

    private static string GetFixturePath(string folder, string file)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "Etl.Sources", "Fixtures", "ReferenceData", folder, file);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Fixture not found: {folder}/{file}");
    }
}
