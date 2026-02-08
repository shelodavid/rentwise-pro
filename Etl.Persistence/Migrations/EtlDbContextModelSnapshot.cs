using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentWisePro.Etl.Persistence.Contexts;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

[DbContext(typeof(EtlDbContext))]
public class EtlDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.EtlRun", entity =>
        {
            entity.ToTable("etl_runs");
            entity.HasKey("RunId");
            entity.Property<Guid>("RunId");
            entity.Property<DateTimeOffset>("StartedAt");
            entity.Property<DateTimeOffset?>("FinishedAt");
            entity.Property<string>("Status").HasMaxLength(50);
            entity.Property<string>("Notes");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.Property", entity =>
        {
            entity.ToTable("properties");
            entity.HasKey("PropertyId");
            entity.Property<Guid>("PropertyId");
            entity.Property<string>("NormalizedAddress").HasMaxLength(500);
            entity.Property<string>("NormalizedAddressHash").HasMaxLength(128);
            entity.Property<string>("OriginalAddress").HasMaxLength(500);
            entity.Property<string>("Street").HasMaxLength(255);
            entity.Property<string>("City").HasMaxLength(100);
            entity.Property<string>("State").HasMaxLength(50);
            entity.Property<string>("Zip").HasMaxLength(20);
            entity.Property<decimal?>("Latitude").HasPrecision(9, 6);
            entity.Property<decimal?>("Longitude").HasPrecision(9, 6);
            entity.Property<string>("PropertyType").HasMaxLength(100);
            entity.Property<int?>("YearBuilt");
            entity.Property<int?>("SquareFeet");
            entity.Property<decimal?>("Beds").HasPrecision(4, 1);
            entity.Property<decimal?>("Baths").HasPrecision(4, 1);
            entity.Property<decimal?>("EstimatedMonthlyRent").HasColumnType("decimal(18,2)");
            entity.Property<string>("RentEstimateSource").HasMaxLength(50);
            entity.Property<DateTimeOffset?>("RentEstimateAsOf");
            entity.Property<int>("NormalizationVersion");
            entity.Property<DateTimeOffset>("CreatedAt");
            entity.Property<DateTimeOffset>("UpdatedAt");
            entity.HasIndex("NormalizedAddressHash").IsUnique();
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.GeoMarketStat", entity =>
        {
            entity.ToTable("geo_market_stats");
            entity.HasKey("GeoType", "GeoKey", "Year");
            entity.Property<string>("GeoType").HasMaxLength(20);
            entity.Property<string>("GeoKey").HasMaxLength(20);
            entity.Property<int>("Year");
            entity.Property<decimal>("VacancyRate").HasColumnType("decimal(6,3)");
            entity.Property<decimal>("MedianHouseholdIncome").HasColumnType("decimal(18,2)");
            entity.Property<string>("Source").HasMaxLength(50).HasDefaultValue("ACS");
            entity.Property<DateTimeOffset>("RetrievedAt");
            entity.HasIndex("GeoType", "GeoKey", "Year");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.HudFairMarketRent", entity =>
        {
            entity.ToTable("hud_fmr");
            entity.HasKey("GeoType", "GeoKey", "Year", "Bedrooms");
            entity.Property<string>("GeoType").HasMaxLength(20);
            entity.Property<string>("GeoKey").HasMaxLength(20);
            entity.Property<int>("Year");
            entity.Property<int>("Bedrooms");
            entity.Property<decimal>("Fmr").HasColumnType("decimal(18,2)");
            entity.Property<string>("Source").HasMaxLength(50).HasDefaultValue("HUD");
            entity.Property<DateTimeOffset>("RetrievedAt");
            entity.HasIndex("GeoType", "GeoKey", "Year");
            entity.HasIndex("GeoType", "GeoKey", "Year", "Bedrooms");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.RawPayloadRef", entity =>
        {
            entity.ToTable("raw_payload_refs");
            entity.HasKey("RawRef");
            entity.Property<string>("RawRef").HasMaxLength(500);
            entity.Property<string>("Source").HasMaxLength(100);
            entity.Property<string>("SourceListingId").HasMaxLength(200);
            entity.Property<DateTimeOffset>("FetchedAt");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.EtlRunSourceStat", entity =>
        {
            entity.ToTable("etl_run_source_stats");
            entity.HasKey("RunId", "Source");
            entity.Property<Guid>("RunId");
            entity.Property<string>("Source").HasMaxLength(100);
            entity.Property<int>("ListingsFetched");
            entity.Property<int>("ListingsUpserted");
            entity.Property<int>("SnapshotsCreated");
            entity.Property<int>("RawPayloadsSaved");
            entity.Property<int>("Errors");
            entity.Property<long>("DurationMs");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.Listing", entity =>
        {
            entity.ToTable("listings");
            entity.HasKey("ListingId");
            entity.Property<Guid>("ListingId");
            entity.Property<Guid>("PropertyId");
            entity.Property<string>("Source").HasMaxLength(100);
            entity.Property<string>("SourceListingId").HasMaxLength(200);
            entity.Property<string>("Status").HasMaxLength(50);
            entity.Property<decimal?>("Price").HasColumnType("decimal(18,0)");
            entity.Property<string>("Currency").HasMaxLength(10).HasDefaultValue("USD");
            entity.Property<DateTimeOffset>("FirstSeenAt");
            entity.Property<DateTimeOffset>("LastSeenAt");
            entity.Property<DateTimeOffset?>("SoldAt");
            entity.Property<string>("MaterialHash").HasMaxLength(128);
            entity.Property<int>("MissingRuns");
            entity.Property<DateTimeOffset>("CreatedAt");
            entity.Property<DateTimeOffset>("UpdatedAt");
            entity.HasIndex("PropertyId");
            entity.HasIndex("Status");
            entity.HasIndex("LastSeenAt");
            entity.HasIndex("Source", "SourceListingId").IsUnique();
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.PropertyPhoto", entity =>
        {
            entity.ToTable("property_photos");
            entity.HasKey("PhotoId");
            entity.Property<Guid>("PhotoId");
            entity.Property<Guid>("PropertyId");
            entity.Property<string>("Source").HasMaxLength(100);
            entity.Property<int>("PhotoIndex");
            entity.Property<string>("UrlOriginal").HasMaxLength(1000);
            entity.Property<string>("StoragePath").HasMaxLength(500);
            entity.Property<string>("Checksum").HasMaxLength(128);
            entity.Property<int?>("Width");
            entity.Property<int?>("Height");
            entity.Property<DateTimeOffset>("CreatedAt");
            entity.HasIndex("PropertyId", "Source", "PhotoIndex").IsUnique();
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.RentForecast", entity =>
        {
            entity.ToTable("rent_forecasts");
            entity.HasKey("ForecastId");
            entity.Property<Guid>("ForecastId");
            entity.Property<Guid>("PropertyId");
            entity.Property<Guid?>("ListingId");
            entity.Property<string>("Source").HasMaxLength(100);
            entity.Property<decimal>("EstimatedRent").HasColumnType("decimal(18,2)");
            entity.Property<bool>("IsStub");
            entity.Property<DateTimeOffset>("CreatedAt");
            entity.Property<DateTimeOffset>("UpdatedAt");
            entity.HasIndex("PropertyId", "Source");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.WorkQueueItem", entity =>
        {
            entity.ToTable("work_queue");
            entity.HasKey("WorkId");
            entity.Property<Guid>("WorkId");
            entity.Property<string>("WorkType").HasMaxLength(100);
            entity.Property<Guid>("PropertyId");
            entity.Property<Guid?>("ListingId");
            entity.Property<string>("PayloadJson").HasColumnType("nvarchar(max)");
            entity.Property<string>("Status").HasMaxLength(50);
            entity.Property<int>("Attempts");
            entity.Property<DateTimeOffset>("AvailableAt");
            entity.Property<DateTimeOffset>("CreatedAt");
            entity.Property<DateTimeOffset>("UpdatedAt");
            entity.HasIndex("Status", "AvailableAt");
        });

        modelBuilder.Entity("RentWisePro.Etl.Core.Entities.ListingSnapshot", entity =>
        {
            entity.ToTable("listing_snapshots");
            entity.HasKey("SnapshotId");
            entity.Property<Guid>("SnapshotId");
            entity.Property<Guid>("ListingId");
            entity.Property<string>("Status").HasMaxLength(50);
            entity.Property<decimal?>("Price").HasColumnType("decimal(18,0)");
            entity.Property<string>("MaterialHash").HasMaxLength(128);
            entity.Property<DateTimeOffset>("ScrapedAt");
            entity.Property<string>("RawRef");
            entity.HasIndex("ListingId", "ScrapedAt");
        });
    }
}
