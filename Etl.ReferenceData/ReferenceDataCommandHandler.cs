using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.ReferenceData;

public class ReferenceDataCommandHandler
{
    private readonly ReferenceDataPaths _paths;
    private readonly ReferenceDataDownloader _downloader;
    private readonly HudFmrReferenceImporter _hudImporter;
    private readonly GeoMarketStatsImporter _acsImporter;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReferenceDataCommandHandler> _logger;

    public ReferenceDataCommandHandler(
        ReferenceDataPaths paths,
        ReferenceDataDownloader downloader,
        HudFmrReferenceImporter hudImporter,
        GeoMarketStatsImporter acsImporter,
        IConfiguration configuration,
        ILogger<ReferenceDataCommandHandler> logger)
    {
        _paths = paths;
        _downloader = downloader;
        _hudImporter = hudImporter;
        _acsImporter = acsImporter;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task HandleAsync(ReferenceDataCommand command, CancellationToken cancellationToken)
    {
        var geoType = string.IsNullOrWhiteSpace(command.GeoType) ? GeoTypes.Zip : command.GeoType.ToUpperInvariant();
        var sourcePath = command.SourcePath;

        if (command.Sample)
        {
            sourcePath = command.ImportKind == ReferenceImportKind.HudFmr
                ? Path.Combine(_paths.GetFixtureRoot(), "Hud", "hud_fmr_sample.csv")
                : Path.Combine(_paths.GetFixtureRoot(), "Acs", "geo_market_stats_sample.csv");
        }
        else if (string.IsNullOrWhiteSpace(sourcePath))
        {
            var storageRoot = _paths.GetStorageRoot(_configuration);
            var fileName = command.ImportKind == ReferenceImportKind.HudFmr
                ? $"hud_fmr_{command.Year}_{geoType.ToLowerInvariant()}.csv"
                : $"geo_market_stats_{command.Year}_{geoType.ToLowerInvariant()}.csv";
            var folder = command.ImportKind == ReferenceImportKind.HudFmr ? "hud" : "acs";
            var destinationPath = Path.Combine(storageRoot, "reference", folder, fileName);
            sourcePath = await _downloader.GetOrDownloadAsync(
                command.DownloadUrl,
                destinationPath,
                command.ManualDownload,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            _logger.LogWarning("No source file available for import.");
            return;
        }

        if (command.ImportKind == ReferenceImportKind.HudFmr)
        {
            var imported = await _hudImporter.ImportAsync(sourcePath, command.Year, geoType, cancellationToken);
            _logger.LogInformation("HUD FMR import completed. Rows processed: {Rows}.", imported);
            return;
        }

        var acsImported = await _acsImporter.ImportAsync(sourcePath, command.Year, geoType, cancellationToken);
        _logger.LogInformation("ACS import completed. Rows processed: {Rows}.", acsImported);
    }
}
