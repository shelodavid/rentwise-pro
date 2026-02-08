using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Services;

public class HudFmrImportService
{
    private readonly EtlDbContext _dbContext;
    private readonly ILogger<HudFmrImportService> _logger;

    public HudFmrImportService(EtlDbContext dbContext, ILogger<HudFmrImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> ImportAsync(string csvPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("HUD FMR CSV not found at {Path}.", csvPath);
            return 0;
        }

        var lines = await File.ReadAllLinesAsync(csvPath, cancellationToken);
        if (lines.Length <= 1)
        {
            _logger.LogWarning("HUD FMR CSV at {Path} does not contain data rows.", csvPath);
            return 0;
        }

        var headers = lines[0].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var headerIndex = BuildHeaderIndex(headers);
        var now = DateTimeOffset.UtcNow;
        var processed = 0;

        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            var values = lines[i].Split(',', StringSplitOptions.TrimEntries);
            if (!TryGet(values, headerIndex, "Year", out var yearText) ||
                !int.TryParse(yearText, out var year))
            {
                continue;
            }

            if (!TryGet(values, headerIndex, "GeoCode", out var geoCode) || string.IsNullOrWhiteSpace(geoCode))
            {
                continue;
            }

            if (!TryGet(values, headerIndex, "Bedrooms", out var bedroomsText) ||
                !int.TryParse(bedroomsText, out var bedrooms))
            {
                continue;
            }

            if (!TryGet(values, headerIndex, "FmrMonthlyRent", out var rentText) ||
                !decimal.TryParse(rentText, out var rent))
            {
                continue;
            }

            var source = TryGet(values, headerIndex, "Source", out var sourceText) && !string.IsNullOrWhiteSpace(sourceText)
                ? sourceText
                : "HUD";

            var existing = await _dbContext.HudFairMarketRents.FindAsync(
                new object[] { year, geoCode, bedrooms },
                cancellationToken);

            if (existing is null)
            {
                existing = new HudFairMarketRent
                {
                    Year = year,
                    GeoCode = geoCode,
                    Bedrooms = bedrooms,
                    FmrMonthlyRent = rent,
                    Source = source,
                    ImportedAt = now
                };
                _dbContext.HudFairMarketRents.Add(existing);
            }
            else
            {
                existing.FmrMonthlyRent = rent;
                existing.Source = source;
                existing.ImportedAt = now;
            }

            processed += 1;
        }

        if (processed > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }

    private static Dictionary<string, int> BuildHeaderIndex(string[] headers)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Length; i++)
        {
            index[headers[i]] = i;
        }

        return index;
    }

    private static bool TryGet(string[] values, Dictionary<string, int> headerIndex, string column, out string? value)
    {
        if (headerIndex.TryGetValue(column, out var index) && index < values.Length)
        {
            value = values[index];
            return true;
        }

        value = null;
        return false;
    }
}
