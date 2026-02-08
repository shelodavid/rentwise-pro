using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.Core.Interfaces;

public interface IEtlRepository
{
    Task<Guid> StartRunAsync(DateTimeOffset startedAt, CancellationToken cancellationToken);
    Task CompleteRunAsync(Guid runId, string status, string? notes, DateTimeOffset finishedAt, CancellationToken cancellationToken);
    Task RecordSourceStatsAsync(EtlRunSourceStat stat, CancellationToken cancellationToken);
    Task AddRawPayloadRefAsync(RawPayloadRef rawPayloadRef, CancellationToken cancellationToken);
    Task<Property> GetOrCreatePropertyAsync(SourceListing listing, string normalizedAddress, string normalizedHash, CancellationToken cancellationToken);
    Task UpdateRentEstimateAsync(Property property, RentEstimate estimate, CancellationToken cancellationToken);
    Task<ListingUpsertResult> UpsertListingAsync(Property property, string source, SourceListing listing, string materialHash, DateTimeOffset seenAt, CancellationToken cancellationToken);
    Task<ListingSnapshot?> AddSnapshotIfChangedAsync(Listing listing, string materialHash, string? rawRef, DateTimeOffset scrapedAt, CancellationToken cancellationToken);
    Task EnqueueWorkItemAsync(WorkQueueItem item, CancellationToken cancellationToken);
    Task MarkMissingListingsAsync(string source, DateTimeOffset runStartedAt, int missingRunsThreshold, CancellationToken cancellationToken);
}
