using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Options;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Services;

public class PhotoDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly IPhotoStorage _photoStorage;
    private readonly EtlDbContext _dbContext;
    private readonly ILogger<PhotoDownloadService> _logger;
    private readonly EtlOptions _etlOptions;

    public PhotoDownloadService(
        HttpClient httpClient,
        IPhotoStorage photoStorage,
        EtlDbContext dbContext,
        IOptions<EtlOptions> etlOptions,
        ILogger<PhotoDownloadService> logger)
    {
        _httpClient = httpClient;
        _photoStorage = photoStorage;
        _dbContext = dbContext;
        _etlOptions = etlOptions.Value;
        _logger = logger;
    }

    public async Task<PhotoDownloadResult> DownloadAsync(
        Guid propertyId,
        string source,
        IReadOnlyList<string> photoUrls,
        CancellationToken cancellationToken)
    {
        var requests = BuildRequests(photoUrls, _etlOptions.MaxPhotosPerProperty);
        if (requests.Count == 0)
        {
            return new PhotoDownloadResult(0, 0, 0, Array.Empty<string>());
        }

        var concurrency = _etlOptions.PhotoDownloadConcurrency > 0 ? _etlOptions.PhotoDownloadConcurrency : 4;
        using var semaphore = new SemaphoreSlim(concurrency, concurrency);
        var tasks = requests.Select(request => DownloadPhotoAsync(request, semaphore, cancellationToken)).ToArray();
        var attempts = await Task.WhenAll(tasks);

        var failures = new List<string>();
        foreach (var attempt in attempts)
        {
            if (attempt.Content is null)
            {
                failures.Add(attempt.ErrorMessage ?? $"Photo download failed for {attempt.Url}.");
                _logger.LogWarning("Photo download failed for {Url}: {Reason}", attempt.Url, attempt.ErrorMessage);
                continue;
            }

            var checksum = ComputeChecksum(attempt.Content);

            var existing = await _dbContext.PropertyPhotos
                .FirstOrDefaultAsync(
                    p => p.PropertyId == propertyId && p.Source == source && p.PhotoIndex == attempt.Index,
                    cancellationToken);

            if (existing is not null && string.Equals(existing.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var duplicate = await _dbContext.PropertyPhotos.FirstOrDefaultAsync(
                p => p.PropertyId == propertyId && p.Source == source && p.Checksum == checksum,
                cancellationToken);

            if (duplicate is not null)
            {
                _logger.LogInformation(
                    "Skipping duplicate photo checksum {Checksum} for property {PropertyId} (source={Source})",
                    checksum,
                    propertyId,
                    source);
                continue;
            }

            var storagePath = await _photoStorage.SaveAsync(propertyId, source, attempt.Index, attempt.Content, cancellationToken);
            if (existing is null)
            {
                existing = new PropertyPhoto
                {
                    PhotoId = Guid.NewGuid(),
                    PropertyId = propertyId,
                    Source = source,
                    PhotoIndex = attempt.Index,
                    UrlOriginal = attempt.Url,
                    StoragePath = storagePath,
                    Checksum = checksum,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.PropertyPhotos.Add(existing);
            }
            else
            {
                existing.UrlOriginal = attempt.Url;
                existing.StoragePath = storagePath;
                existing.Checksum = checksum;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var total = attempts.Length;
        var succeeded = attempts.Count(attempt => attempt.Content is not null);
        return new PhotoDownloadResult(total, succeeded, failures.Count, failures);
    }

    private static List<PhotoDownloadRequest> BuildRequests(IReadOnlyList<string> photoUrls, int maxPhotos)
    {
        var results = new List<PhotoDownloadRequest>();
        if (photoUrls.Count == 0)
        {
            return results;
        }

        var limit = maxPhotos > 0 ? maxPhotos : 10;
        for (var index = 0; index < photoUrls.Count && results.Count < limit; index++)
        {
            var url = photoUrls[index];
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            results.Add(new PhotoDownloadRequest(index, url));
        }

        return results;
    }

    private async Task<PhotoDownloadAttempt> DownloadPhotoAsync(
        PhotoDownloadRequest request,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var response = await _httpClient.GetAsync(request.Url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new PhotoDownloadAttempt(
                    request.Index,
                    request.Url,
                    null,
                    $"Status {(int)response.StatusCode} {response.ReasonPhrase}".Trim());
            }

            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return new PhotoDownloadAttempt(request.Index, request.Url, content, null);
        }
        catch (Exception ex)
        {
            return new PhotoDownloadAttempt(request.Index, request.Url, null, ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string ComputeChecksum(byte[] content)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(content)).ToLowerInvariant();
    }

    public sealed record PhotoDownloadResult(int Total, int Succeeded, int Failed, IReadOnlyList<string> Errors);

    private sealed record PhotoDownloadRequest(int Index, string Url);

    private sealed record PhotoDownloadAttempt(int Index, string Url, byte[]? Content, string? ErrorMessage);
}
