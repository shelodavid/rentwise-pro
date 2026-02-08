using Microsoft.EntityFrameworkCore;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Persistence.Contexts;

namespace RentWisePro.Etl.Persistence.Repositories;

public class EtlRepository : IEtlRepository
{
    private readonly EtlDbContext _dbContext;

    public EtlRepository(EtlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> StartRunAsync(DateTimeOffset startedAt, CancellationToken cancellationToken)
    {
        var run = new EtlRun
        {
            RunId = Guid.NewGuid(),
            StartedAt = startedAt,
            Status = "Running"
        };

        _dbContext.EtlRuns.Add(run);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return run.RunId;
    }

    public async Task CompleteRunAsync(Guid runId, string status, string? notes, DateTimeOffset finishedAt, CancellationToken cancellationToken)
    {
        var run = await _dbContext.EtlRuns.FirstOrDefaultAsync(r => r.RunId == runId, cancellationToken);
        if (run is null)
        {
            return;
        }

        run.Status = status;
        run.Notes = notes;
        run.FinishedAt = finishedAt;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordSourceStatsAsync(EtlRunSourceStat stat, CancellationToken cancellationToken)
    {
        _dbContext.EtlRunSourceStats.Add(stat);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRawPayloadRefAsync(RawPayloadRef rawPayloadRef, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.RawPayloadRefs
            .AnyAsync(r => r.RawRef == rawPayloadRef.RawRef, cancellationToken);
        if (!exists)
        {
            _dbContext.RawPayloadRefs.Add(rawPayloadRef);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Property> GetOrCreatePropertyAsync(SourceListing listing, string normalizedAddress, string normalizedHash, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Properties
            .FirstOrDefaultAsync(p => p.NormalizedAddressHash == normalizedHash, cancellationToken);

        if (existing is not null)
        {
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.Latitude = listing.Latitude ?? existing.Latitude;
            existing.Longitude = listing.Longitude ?? existing.Longitude;
            existing.PropertyType = listing.PropertyType ?? existing.PropertyType;
            existing.YearBuilt = listing.YearBuilt ?? existing.YearBuilt;
            existing.SquareFeet = listing.SquareFeet ?? existing.SquareFeet;
            existing.Beds = listing.Beds ?? existing.Beds;
            existing.Baths = listing.Baths ?? existing.Baths;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var property = new Property
        {
            PropertyId = Guid.NewGuid(),
            NormalizedAddress = normalizedAddress,
            NormalizedAddressHash = normalizedHash,
            OriginalAddress = listing.Address,
            Street = listing.Address,
            City = listing.City,
            State = listing.State,
            Zip = listing.Zip,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            PropertyType = listing.PropertyType,
            YearBuilt = listing.YearBuilt,
            SquareFeet = listing.SquareFeet,
            Beds = listing.Beds,
            Baths = listing.Baths,
            NormalizationVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Properties.Add(property);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return property;
    }

    public async Task UpdateRentEstimateAsync(Property property, RentEstimate estimate, CancellationToken cancellationToken)
    {
        property.EstimatedMonthlyRent = estimate.MonthlyRent;
        property.RentEstimateSource = estimate.Source;
        property.RentEstimateAsOf = estimate.AsOf;
        property.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ListingUpsertResult> UpsertListingAsync(
        Property property,
        string source,
        SourceListing listing,
        string materialHash,
        DateTimeOffset seenAt,
        ListingInvestmentMetrics metrics,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Listings
            .FirstOrDefaultAsync(l => l.Source == source && l.SourceListingId == listing.SourceListingId, cancellationToken);

        if (existing is not null)
        {
            var previousHash = existing.MaterialHash;
            existing.Status = listing.Status ?? existing.Status;
            existing.Price = listing.Price ?? existing.Price;
            existing.EstimatedRent = metrics.EstimatedRent;
            existing.RprMonthly = metrics.RprMonthly;
            existing.Grm = metrics.Grm;
            existing.EstimatedCashFlow = metrics.EstimatedCashFlow;
            existing.AffordabilityIndex = metrics.AffordabilityIndex;
            existing.PricePerSqft = metrics.PricePerSqft;
            existing.LastSeenAt = seenAt;
            existing.MaterialHash = materialHash;
            existing.MissingRuns = 0;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new ListingUpsertResult(existing, previousHash);
        }

        var entity = new Listing
        {
            ListingId = Guid.NewGuid(),
            PropertyId = property.PropertyId,
            Source = source,
            SourceListingId = listing.SourceListingId,
            Status = listing.Status ?? "active",
            Price = listing.Price,
            EstimatedRent = metrics.EstimatedRent,
            RprMonthly = metrics.RprMonthly,
            Grm = metrics.Grm,
            EstimatedCashFlow = metrics.EstimatedCashFlow,
            AffordabilityIndex = metrics.AffordabilityIndex,
            PricePerSqft = metrics.PricePerSqft,
            Currency = "USD",
            FirstSeenAt = seenAt,
            LastSeenAt = seenAt,
            MaterialHash = materialHash,
            MissingRuns = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Listings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ListingUpsertResult(entity, null);
    }

    public async Task<ListingSnapshot?> AddSnapshotIfChangedAsync(Listing listing, string materialHash, string? rawRef, DateTimeOffset scrapedAt, CancellationToken cancellationToken)
    {
        var snapshot = new ListingSnapshot
        {
            SnapshotId = Guid.NewGuid(),
            ListingId = listing.ListingId,
            Status = listing.Status,
            Price = listing.Price,
            MaterialHash = materialHash,
            ScrapedAt = scrapedAt,
            RawRef = rawRef
        };

        _dbContext.ListingSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    public async Task EnqueueWorkItemAsync(WorkQueueItem item, CancellationToken cancellationToken)
    {
        _dbContext.WorkQueue.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkMissingListingsAsync(string source, DateTimeOffset runStartedAt, int missingRunsThreshold, CancellationToken cancellationToken)
    {
        var listings = await _dbContext.Listings
            .Where(l => l.Source == source && l.LastSeenAt < runStartedAt)
            .ToListAsync(cancellationToken);

        foreach (var listing in listings)
        {
            listing.MissingRuns += 1;
            if (listing.MissingRuns >= missingRunsThreshold)
            {
                listing.Status = "removed";
            }
            listing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
