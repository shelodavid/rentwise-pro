using System.Globalization;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.ReferenceData;

public class HudFmrReferenceImporter
{
    private readonly EtlDbContext _dbContext;
    private readonly ILogger<HudFmrReferenceImporter> _logger;

    public HudFmrReferenceImporter(EtlDbContext dbContext, ILogger<HudFmrReferenceImporter> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> ImportAsync(string csvPath, int yearOverride, string geoTypeOverride, CancellationToken cancellationToken)
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
        var defaultGeoType = string.IsNullOrWhiteSpace(geoTypeOverride) ? GeoTypes.Zip : geoTypeOverride.ToUpperInvariant();

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
                if (yearOverride > 0)
                {
                    year = yearOverride;
                }
                else
                {
                    continue;
                }
            }

            if (yearOverride > 0 && year != yearOverride)
            {
                continue;
            }

            if (!TryGet(values, headerIndex, "GeoKey", out var geoKey) || string.IsNullOrWhiteSpace(geoKey))
            {
                if (!TryGet(values, headerIndex, "GeoCode", out geoKey) || string.IsNullOrWhiteSpace(geoKey))
                {
                    continue;
                }
            }

            if (!TryGet(values, headerIndex, "Bedrooms", out var bedroomsText) ||
                !int.TryParse(bedroomsText, out var bedrooms))
            {
                continue;
            }

            if (!TryGet(values, headerIndex, "Fmr", out var rentText) ||
                !decimal.TryParse(rentText, NumberStyles.Number, CultureInfo.InvariantCulture, out var rent))
            {
                if (!TryGet(values, headerIndex, "FmrMonthlyRent", out rentText) ||
                    !decimal.TryParse(rentText, NumberStyles.Number, CultureInfo.InvariantCulture, out rent))
                {
                    continue;
                }
            }

            var geoType = TryGet(values, headerIndex, "GeoType", out var geoTypeText) && !string.IsNullOrWhiteSpace(geoTypeText)
                ? geoTypeText.ToUpperInvariant()
                : defaultGeoType;

            var source = TryGet(values, headerIndex, "Source", out var sourceText) && !string.IsNullOrWhiteSpace(sourceText)
                ? sourceText
                : "HUD";

            var retrievedAt = now;
            if (TryGet(values, headerIndex, "RetrievedAt", out var retrievedText) &&
                DateTimeOffset.TryParse(retrievedText, out var parsedRetrieved))
            {
                retrievedAt = parsedRetrieved;
            }

            var existing = await _dbContext.HudFairMarketRents.FindAsync(
                new object[] { geoType, geoKey, year, bedrooms },
                cancellationToken);

            if (existing is null)
            {
                existing = new HudFairMarketRent
                {
                    Year = year,
                    GeoKey = geoKey,
                    GeoType = geoType,
                    Bedrooms = bedrooms,
                    Fmr = rent,
                    Source = source,
                    RetrievedAt = retrievedAt
                };
                _dbContext.HudFairMarketRents.Add(existing);
            }
            else
            {
                existing.Fmr = rent;
                existing.Source = source;
                existing.RetrievedAt = retrievedAt;
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
