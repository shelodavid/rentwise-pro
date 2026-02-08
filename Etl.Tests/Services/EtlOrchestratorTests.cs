using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Options;
using RentWisePro.Etl.Core.Services;
using Xunit;

namespace RentWisePro.Etl.Tests.Services;

public class EtlOrchestratorTests
{
    [Fact]
    public async Task RunAsync_TrimsPhotoPayloadToConfiguredMax()
    {
        var source = new FakeListingSource();
        var repository = new FakeEtlRepository();
        var rawPayloadStore = new FakeRawPayloadStore();
        var rentEstimators = new[] { new FakeRentEstimator() };
        var options = Options.Create(new EtlOptions { MaxPhotosPerProperty = 10 });
        var orchestrator = new EtlOrchestrator(
            new[] { source },
            repository,
            rawPayloadStore,
            rentEstimators,
            new AddressNormalizer(),
            new HashingService(),
            new MaterialHashBuilder(new HashingService()),
            new SnapshotDecider(),
            options,
            NullLogger<EtlOrchestrator>.Instance);

        await orchestrator.RunAsync(new EtlRunRequest(null, null, 50), CancellationToken.None);

        var photoWork = Assert.Single(repository.WorkItems, item => item.WorkType == "photo_download");
        using var document = JsonDocument.Parse(photoWork.PayloadJson);
        var photos = document.RootElement.GetProperty("photos");
        Assert.Equal(10, photos.GetArrayLength());
    }

    private sealed class FakeListingSource : IListingSource
    {
        private bool _served;

        public string Name => "Fixture Listings";

        public Task<IReadOnlyList<SourceListing>> FetchListingsAsync(SourceFetchRequest request, CancellationToken cancellationToken)
        {
            if (_served)
            {
                return Task.FromResult((IReadOnlyList<SourceListing>)Array.Empty<SourceListing>());
            }

            _served = true;
            var listing = new SourceListing
            {
                SourceListingId = "TEST-1",
                Address = "123 Main St",
                City = "Austin",
                State = "TX",
                Zip = "78701",
                PhotoUrls = Enumerable.Range(1, 12).Select(index => $"https://example.com/{index}.jpg").ToList(),
                RawJson = "{}"
            };

            return Task.FromResult((IReadOnlyList<SourceListing>)new[] { listing });
        }
    }

    private sealed class FakeRawPayloadStore : IRawPayloadStore
    {
        public Task<string> SaveAsync(string source, string sourceListingId, string rawJson, CancellationToken cancellationToken)
        {
            return Task.FromResult("raw-ref");
        }
    }

    private sealed class FakeRentEstimator : IRentEstimator
    {
        public int Priority => 0;

        public Task<RentEstimate?> EstimateAsync(Property property, CancellationToken cancellationToken)
        {
            return Task.FromResult<RentEstimate?>(null);
        }
    }

    private sealed class FakeEtlRepository : IEtlRepository
    {
        public List<WorkQueueItem> WorkItems { get; } = new();

        public Task<Guid> StartRunAsync(DateTimeOffset startedAt, CancellationToken cancellationToken)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task CompleteRunAsync(Guid runId, string status, string? notes, DateTimeOffset finishedAt, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RecordSourceStatsAsync(EtlRunSourceStat stat, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task AddRawPayloadRefAsync(RawPayloadRef rawPayloadRef, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<Property> GetOrCreatePropertyAsync(SourceListing listing, string normalizedAddress, string normalizedHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Property { PropertyId = Guid.NewGuid() });
        }

        public Task UpdateRentEstimateAsync(Property property, RentEstimate estimate, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<ListingUpsertResult> UpsertListingAsync(Property property, string source, SourceListing listing, string materialHash, DateTimeOffset seenAt, CancellationToken cancellationToken)
        {
            var entity = new Listing
            {
                ListingId = Guid.NewGuid(),
                PropertyId = property.PropertyId,
                Source = source,
                SourceListingId = listing.SourceListingId,
                Status = listing.Status ?? "active",
                MaterialHash = materialHash
            };

            return Task.FromResult(new ListingUpsertResult(entity, null));
        }

        public Task<ListingSnapshot?> AddSnapshotIfChangedAsync(Listing listing, string materialHash, string? rawRef, DateTimeOffset scrapedAt, CancellationToken cancellationToken)
        {
            return Task.FromResult<ListingSnapshot?>(null);
        }

        public Task EnqueueWorkItemAsync(WorkQueueItem item, CancellationToken cancellationToken)
        {
            WorkItems.Add(item);
            return Task.CompletedTask;
        }

        public Task MarkMissingListingsAsync(string source, DateTimeOffset runStartedAt, int missingRunsThreshold, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
