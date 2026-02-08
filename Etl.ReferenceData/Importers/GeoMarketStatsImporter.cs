using System.Globalization;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.ReferenceData;

public class GeoMarketStatsImporter
{
    private readonly EtlDbContext _dbContext;
    private readonly ILogger<GeoMarketStatsImporter> _logger;

    public GeoMarketStatsImporter(EtlDbContext dbContext, ILogger<GeoMarketStatsImporter> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> ImportAsync(string csvPath, int yearOverride, string geoTypeOverride, CancellationToken cancellationToken)
    {
        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("ACS CSV not found at {Path}.", csvPath);
            return 0;
        }

        var lines = await File.ReadAllLinesAsync(csvPath, cancellationToken);
        if (lines.Length <= 1)
        {
            _logger.LogWarning("ACS CSV at {Path} does not contain data rows.", csvPath);
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
                continue;
            }

            var geoType = TryGet(values, headerIndex, "GeoType", out var geoTypeText) && !string.IsNullOrWhiteSpace(geoTypeText)
                ? geoTypeText.ToUpperInvariant()
                : defaultGeoType;

            if (!TryGet(values, headerIndex, "VacancyRate", out var vacancyText) ||
                !decimal.TryParse(vacancyText, NumberStyles.Number, CultureInfo.InvariantCulture, out var vacancy))
            {
                continue;
            }

            if (!TryGet(values, headerIndex, "MedianHouseholdIncome", out var incomeText) ||
                !decimal.TryParse(incomeText, NumberStyles.Number, CultureInfo.InvariantCulture, out var income))
            {
                continue;
            }

            var source = TryGet(values, headerIndex, "Source", out var sourceText) && !string.IsNullOrWhiteSpace(sourceText)
                ? sourceText
                : "ACS";

            var retrievedAt = now;
            if (TryGet(values, headerIndex, "RetrievedAt", out var retrievedText) &&
                DateTimeOffset.TryParse(retrievedText, out var parsedRetrieved))
            {
                retrievedAt = parsedRetrieved;
            }

            var existing = await _dbContext.GeoMarketStats.FindAsync(
                new object[] { geoType, geoKey, year },
                cancellationToken);

            if (existing is null)
            {
                existing = new GeoMarketStat
                {
                    Year = year,
                    GeoKey = geoKey,
                    GeoType = geoType,
                    VacancyRate = vacancy,
                    MedianHouseholdIncome = income,
                    Source = source,
                    RetrievedAt = retrievedAt
                };
                _dbContext.GeoMarketStats.Add(existing);
            }
            else
            {
                existing.VacancyRate = vacancy;
                existing.MedianHouseholdIncome = income;
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
