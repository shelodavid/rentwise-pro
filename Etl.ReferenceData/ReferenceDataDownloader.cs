using System.Net;
using Microsoft.Extensions.Logging;

namespace RentWisePro.Etl.ReferenceData;

public class ReferenceDataDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReferenceDataDownloader> _logger;

    public ReferenceDataDownloader(IHttpClientFactory httpClientFactory, ILogger<ReferenceDataDownloader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> GetOrDownloadAsync(string? downloadUrl, string destinationPath, bool manualOnly, CancellationToken cancellationToken)
    {
        if (File.Exists(destinationPath))
        {
            _logger.LogInformation("Using cached reference data at {Path}.", destinationPath);
            return destinationPath;
        }

        if (manualOnly || string.IsNullOrWhiteSpace(downloadUrl))
        {
            _logger.LogWarning("Reference data missing. Place file at {Path} or provide --download-url.", destinationPath);
            return null;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        var client = _httpClientFactory.CreateClient();

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(downloadUrl, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Download URL returned 404: {Url}.", downloadUrl);
                    return null;
                }

                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var output = File.Create(destinationPath);
                await stream.CopyToAsync(output, cancellationToken);

                _logger.LogInformation("Downloaded reference data to {Path}.", destinationPath);
                return destinationPath;
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "Failed to download reference data (attempt {Attempt}). Retrying.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
            }
        }

        _logger.LogError("Failed to download reference data from {Url}.", downloadUrl);
        return null;
    }
}
