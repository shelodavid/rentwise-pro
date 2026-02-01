using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Options;

namespace RentWisePro.Etl.Core.Services;

public class EtlOrchestrator : IEtlOrchestrator
{
    private readonly IEnumerable<IListingSource> _sources;
    private readonly IEtlRepository _repository;
    private readonly IRawPayloadStore _rawPayloadStore;
    private readonly AddressNormalizer _addressNormalizer;
    private readonly HashingService _hashingService;
    private readonly MaterialHashBuilder _materialHashBuilder;
    private readonly SnapshotDecider _snapshotDecider;
    private readonly EtlOptions _etlOptions;
    private readonly ILogger<EtlOrchestrator> _logger;

    public EtlOrchestrator(
        IEnumerable<IListingSource> sources,
        IEtlRepository repository,
        IRawPayloadStore rawPayloadStore,
        AddressNormalizer addressNormalizer,
        HashingService hashingService,
        MaterialHashBuilder materialHashBuilder,
        SnapshotDecider snapshotDecider,
        IOptions<EtlOptions> etlOptions,
        ILogger<EtlOrchestrator> logger)
    {
        _sources = sources;
        _repository = repository;
        _rawPayloadStore = rawPayloadStore;
        _addressNormalizer = addressNormalizer;
        _hashingService = hashingService;
        _materialHashBuilder = materialHashBuilder;
        _snapshotDecider = snapshotDecider;
        _etlOptions = etlOptions.Value;
        _logger = logger;
    }

    public async Task RunAsync(EtlRunRequest request, CancellationToken cancellationToken)
    {
        var runStartedAt = DateTimeOffset.UtcNow;
        var runId = await _repository.StartRunAsync(runStartedAt, cancellationToken);
        var notes = new List<string>();
        var runStatus = "Completed";

        try
        {
            var sourceList = _sources
                .Where(source => string.IsNullOrWhiteSpace(request.SourceFilter) ||
                                 source.Name.Equals(request.SourceFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var source in sourceList)
            {
                var stats = new EtlRunSourceStat
                {
                    RunId = runId,
                    Source = source.Name
                };

                var sourceStartedAt = DateTimeOffset.UtcNow;
                var page = 1;
                const int maxPages = 500;
                var pageSize = request.PageSize;

                while (!cancellationToken.IsCancellationRequested && page <= maxPages)
                {
                    var listings = await source.FetchListingsAsync(
                        new SourceFetchRequest(page, pageSize, request.Since),
                        cancellationToken);

                    if (listings.Count == 0)
                    {
                        break;
                    }

                    stats.ListingsFetched += listings.Count;

                    foreach (var listing in listings)
                    {
                        if (string.IsNullOrWhiteSpace(listing.SourceListingId))
                        {
                            continue;
                        }

                        var rawRef = await _rawPayloadStore.SaveAsync(source.Name, listing.SourceListingId, listing.RawJson, cancellationToken);
                        await _repository.AddRawPayloadRefAsync(new RawPayloadRef
                        {
                            RawRef = rawRef,
                            Source = source.Name,
                            SourceListingId = listing.SourceListingId,
                            FetchedAt = DateTimeOffset.UtcNow
                        }, cancellationToken);
                        stats.RawPayloadsSaved += 1;

                        var normalizedAddress = _addressNormalizer.Normalize(listing.Address);
                        var addressHash = _hashingService.ComputeSha256($"{normalizedAddress}|{listing.City}|{listing.State}|{listing.Zip}");
                        var property = await _repository.GetOrCreatePropertyAsync(listing, normalizedAddress, addressHash, cancellationToken);

                        var materialHash = _materialHashBuilder.Build(listing);
                        var listingResult = await _repository.UpsertListingAsync(property, source.Name, listing, materialHash, DateTimeOffset.UtcNow, cancellationToken);
                        stats.ListingsUpserted += 1;

                        if (_snapshotDecider.ShouldCreateSnapshot(listingResult.PreviousMaterialHash, materialHash))
                        {
                            var snapshot = await _repository.AddSnapshotIfChangedAsync(listingResult.Listing, materialHash, rawRef, DateTimeOffset.UtcNow, cancellationToken);
                            if (snapshot is not null)
                            {
                                stats.SnapshotsCreated += 1;
                            }
                        }

                        var payload = new
                        {
                            propertyId = property.PropertyId,
                            listingId = listingResult.Listing.ListingId,
                            source = source.Name,
                            photos = listing.PhotoUrls.Take(_etlOptions.MaxPhotosPerProperty).ToArray()
                        };

                        await _repository.EnqueueWorkItemAsync(new WorkQueueItem
                        {
                            WorkId = Guid.NewGuid(),
                            WorkType = "photo_download",
                            PropertyId = property.PropertyId,
                            ListingId = listingResult.Listing.ListingId,
                            PayloadJson = System.Text.Json.JsonSerializer.Serialize(payload),
                            Status = "queued",
                            Attempts = 0,
                            AvailableAt = DateTimeOffset.UtcNow,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        }, cancellationToken);

                        await _repository.EnqueueWorkItemAsync(new WorkQueueItem
                        {
                            WorkId = Guid.NewGuid(),
                            WorkType = "rent_forecast",
                            PropertyId = property.PropertyId,
                            ListingId = listingResult.Listing.ListingId,
                            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { listingId = listingResult.Listing.ListingId }),
                            Status = "queued",
                            Attempts = 0,
                            AvailableAt = DateTimeOffset.UtcNow,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        }, cancellationToken);
                    }

                    if (listings.Count < pageSize)
                    {
                        break;
                    }

                    page += 1;
                }

                await _repository.MarkMissingListingsAsync(source.Name, runStartedAt, _etlOptions.MarkOffMarketAfterMissingRuns, cancellationToken);
                stats.DurationMs = (long)(DateTimeOffset.UtcNow - sourceStartedAt).TotalMilliseconds;
                await _repository.RecordSourceStatsAsync(stats, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            runStatus = "Failed";
            notes.Add(ex.Message);
            _logger.LogError(ex, "ETL run failed");
        }
        finally
        {
            await _repository.CompleteRunAsync(runId, runStatus, notes.Count > 0 ? string.Join("; ", notes) : null, DateTimeOffset.UtcNow, cancellationToken);
        }
    }
}

public record EtlRunRequest(string? SourceFilter, DateTimeOffset? Since, int PageSize);
