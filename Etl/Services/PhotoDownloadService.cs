using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Services;

public class PhotoDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly IPhotoStorage _photoStorage;
    private readonly EtlDbContext _dbContext;
    private readonly ILogger<PhotoDownloadService> _logger;

    public PhotoDownloadService(
        HttpClient httpClient,
        IPhotoStorage photoStorage,
        EtlDbContext dbContext,
        ILogger<PhotoDownloadService> logger)
    {
        _httpClient = httpClient;
        _photoStorage = photoStorage;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task DownloadAsync(Guid propertyId, string source, IReadOnlyList<string> photoUrls, CancellationToken cancellationToken)
    {
        for (var index = 0; index < photoUrls.Count; index++)
        {
            var url = photoUrls[index];
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Photo download failed for {Url} with status {Status}", url, response.StatusCode);
                    continue;
                }

                var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var checksum = ComputeChecksum(content);

                var existing = await _dbContext.PropertyPhotos
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.Source == source && p.PhotoIndex == index, cancellationToken);

                if (existing is not null && string.Equals(existing.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var storagePath = await _photoStorage.SaveAsync(propertyId, source, index, content, cancellationToken);
                if (existing is null)
                {
                    existing = new PropertyPhoto
                    {
                        PhotoId = Guid.NewGuid(),
                        PropertyId = propertyId,
                        Source = source,
                        PhotoIndex = index,
                        UrlOriginal = url,
                        StoragePath = storagePath,
                        Checksum = checksum,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    _dbContext.PropertyPhotos.Add(existing);
                }
                else
                {
                    existing.UrlOriginal = url;
                    existing.StoragePath = storagePath;
                    existing.Checksum = checksum;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Photo download failed for {Url}", url);
            }
        }
    }

    private static string ComputeChecksum(byte[] content)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(content)).ToLowerInvariant();
    }
}
