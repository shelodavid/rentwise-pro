namespace RentWisePro.Etl.ReferenceData;

public sealed record ReferenceDataCommand(
    ReferenceImportKind ImportKind,
    int Year,
    string GeoType,
    bool Sample,
    string? SourcePath,
    string? DownloadUrl,
    bool ManualDownload,
    bool IsValid)
{
    public static ReferenceDataCommand Parse(string[] args)
    {
        if (args.Any(arg => string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)))
        {
            return new ReferenceDataCommand(ReferenceImportKind.Unknown, 0, "ZIP", false, null, null, false, false);
        }

        var importKind = ReferenceImportKind.Unknown;
        var year = 0;
        var geoType = "ZIP";
        var sample = false;
        string? sourcePath = null;
        string? downloadUrl = null;
        var manualDownload = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--import", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                importKind = ParseImportKind(args[++index]);
            }
            else if (arg.StartsWith("--import=", StringComparison.OrdinalIgnoreCase))
            {
                importKind = ParseImportKind(arg.Split('=', 2)[1]);
            }
            else if (string.Equals(arg, "--year", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                int.TryParse(args[++index], out year);
            }
            else if (arg.StartsWith("--year=", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(arg.Split('=', 2)[1], out year);
            }
            else if (string.Equals(arg, "--geo", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                geoType = args[++index];
            }
            else if (arg.StartsWith("--geo=", StringComparison.OrdinalIgnoreCase))
            {
                geoType = arg.Split('=', 2)[1];
            }
            else if (string.Equals(arg, "--sample", StringComparison.OrdinalIgnoreCase))
            {
                sample = true;
            }
            else if (string.Equals(arg, "--source-path", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                sourcePath = args[++index];
            }
            else if (arg.StartsWith("--source-path=", StringComparison.OrdinalIgnoreCase))
            {
                sourcePath = arg.Split('=', 2)[1];
            }
            else if (string.Equals(arg, "--download-url", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                downloadUrl = args[++index];
            }
            else if (arg.StartsWith("--download-url=", StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = arg.Split('=', 2)[1];
            }
            else if (string.Equals(arg, "--manual", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(arg, "--manual-download", StringComparison.OrdinalIgnoreCase))
            {
                manualDownload = true;
            }
        }

        geoType = string.IsNullOrWhiteSpace(geoType) ? "ZIP" : geoType.ToUpperInvariant();
        var isValid = importKind != ReferenceImportKind.Unknown && year > 0;

        return new ReferenceDataCommand(importKind, year, geoType, sample, sourcePath, downloadUrl, manualDownload, isValid);
    }

    private static ReferenceImportKind ParseImportKind(string? value)
    {
        if (string.Equals(value, "hud-fmr", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "hud", StringComparison.OrdinalIgnoreCase))
        {
            return ReferenceImportKind.HudFmr;
        }

        if (string.Equals(value, "acs", StringComparison.OrdinalIgnoreCase))
        {
            return ReferenceImportKind.Acs;
        }

        return ReferenceImportKind.Unknown;
    }

    public static string Usage => string.Join(Environment.NewLine, new[]
    {
        "Reference data importer",
        "",
        "Usage:",
        "  dotnet run --project Etl.ReferenceData -- --import hud-fmr --year 2024 --geo zip --sample",
        "  dotnet run --project Etl.ReferenceData -- --import acs --year 2023 --geo zip --sample",
        "",
        "Options:",
        "  --import <hud-fmr|acs>   Import dataset",
        "  --year <YYYY>            Year to import",
        "  --geo <zip|county>       Geo type (default ZIP)",
        "  --sample                 Use fixture samples from Etl.Sources/Fixtures/ReferenceData",
        "  --source-path <path>     Use a local CSV file instead of downloading",
        "  --download-url <url>     Override download URL",
        "  --manual                 Skip download if cached file is missing",
        "  --help                   Show help"
    });
}

public enum ReferenceImportKind
{
    Unknown = 0,
    HudFmr = 1,
    Acs = 2
}
